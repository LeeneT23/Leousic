using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Representa una capa individual en el proyecto.
/// Cada capa tiene su propia textura, visibilidad, opacidad y modo de fusión.
/// </summary>
public partial class Layer : Resource
{
    [Signal] public delegate void LayerChangedEventHandler();
    
    public enum BlendModes
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        Darken,
        Lighten
    }
    
    [Export] public int Id { get; set; }
    [Export] public string Name { get; set; } = "Capa";
    [Export] public ImageTexture? Texture { get; set; }
    [Export] public bool Visible { get; set; } = true;
    [Export] public float Opacity { get; set; } = 1.0f;
    [Export] public BlendModes BlendMode { get; set; } = BlendModes.Normal;
    [Export] public Vector2 Offset { get; set; } = Vector2.Zero;
    [Export] public bool Locked { get; set; } = false;
    
    private bool _isModified = false;
    
    public bool IsModified => _isModified;
    
    /// <summary>
    /// Crea una nueva capa vacía
    /// </summary>
    public static Layer Create(int id, string name, int width, int height)
    {
        var layer = new Layer
        {
            Id = id,
            Name = name
        };
        
        Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        layer.Texture = ImageTexture.CreateFromImage(image);
        
        return layer;
    }
    
    /// <summary>
    /// Crea una capa desde una imagen existente
    /// </summary>
    public static Layer CreateFromImage(int id, string name, Image image)
    {
        var layer = new Layer
        {
            Id = id,
            Name = name
        };
        
        layer.Texture = ImageTexture.CreateFromImage(image);
        
        return layer;
    }
    
    /// <summary>
    /// Obtiene la imagen subyacente de la textura
    /// </summary>
    public Image GetImage()
    {
        if (Texture == null)
            return new Image();
        
        return Texture.GetImage();
    }
    
    /// <summary>
    /// Actualiza la textura con una nueva imagen
    /// </summary>
    public void UpdateTexture(Image image)
    {
        if (Texture != null)
        {
            Texture.Update(image);
            _isModified = true;
            EmitSignal(SignalName.LayerChanged);
        }
    }
    
    /// <summary>
    /// Limpia el contenido de la capa
    /// </summary>
    public void Clear()
    {
        if (Texture != null)
        {
            Image image = Texture.GetImage();
            image.Fill(Colors.Transparent);
            Texture.Update(image);
            _isModified = true;
            EmitSignal(SignalName.LayerChanged);
        }
    }
    
    /// <summary>
    /// Duplica esta capa
    /// </summary>
    public Layer Duplicate()
    {
        var newLayer = new Layer
        {
            Id = GenerateUniqueId(),
            Name = $"{Name} (copia)",
            Visible = Visible,
            Opacity = Opacity,
            BlendMode = BlendMode,
            Offset = Offset,
            Locked = Locked
        };
        
        if (Texture != null)
        {
            Image image = Texture.GetImage().Duplicate();
            newLayer.Texture = ImageTexture.CreateFromImage(image);
        }
        
        return newLayer;
    }
    
    /// <summary>
    /// Genera un ID único para la capa
    /// </summary>
    private static int GenerateUniqueId()
    {
        return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue);
    }
    
    /// <summary>
    /// Aplica un filtro a la capa
    /// </summary>
    public void ApplyFilter(string filterName)
    {
        if (Texture == null)
            return;
        
        Image image = Texture.GetImage();
        image.Lock();
        
        switch (filterName.ToLower())
        {
            case "grayscale":
                ApplyGrayscale(image);
                break;
            case "invert":
                ApplyInvert(image);
                break;
            case "blur":
                ApplyBlur(image);
                break;
            case "sharpen":
                ApplySharpen(image);
                break;
            case "brightness_up":
                ApplyBrightness(image, 0.2f);
                break;
            case "brightness_down":
                ApplyBrightness(image, -0.2f);
                break;
            case "contrast_up":
                ApplyContrast(image, 0.3f);
                break;
            case "contrast_down":
                ApplyContrast(image, -0.3f);
                break;
        }
        
        image.Unlock();
        Texture.Update(image);
        _isModified = true;
        EmitSignal(SignalName.LayerChanged);
    }
    
    private void ApplyGrayscale(Image image)
    {
        for (int y = 0; y < image.GetSize().Y; y++)
        {
            for (int x = 0; x < image.GetSize().X; x++)
            {
                Color pixel = image.GetPixel(x, y);
                float gray = (pixel.R + pixel.G + pixel.B) / 3;
                image.SetPixel(x, y, new Color(gray, gray, gray, pixel.A));
            }
        }
    }
    
    private void ApplyInvert(Image image)
    {
        for (int y = 0; y < image.GetSize().Y; y++)
        {
            for (int x = 0; x < image.GetSize().X; x++)
            {
                Color pixel = image.GetPixel(x, y);
                image.SetPixel(x, y, new Color(1 - pixel.R, 1 - pixel.G, 1 - pixel.B, pixel.A));
            }
        }
    }
    
    private void ApplyBlur(Image image)
    {
        // Blur simple de 3x3
        Image blurred = image.Duplicate();
        
        for (int y = 1; y < image.GetSize().Y - 1; y++)
        {
            for (int x = 1; x < image.GetSize().X - 1; x++)
            {
                Color sum = Colors.Black;
                
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        sum += image.GetPixel(x + dx, y + dy);
                    }
                }
                
                blurred.SetPixel(x, y, sum / 9);
            }
        }
        
        image.Blend(blurred);
    }
    
    private void ApplySharpen(Image image)
    {
        // Sharpen simple
        Image sharpened = image.Duplicate();
        
        for (int y = 1; y < image.GetSize().Y - 1; y++)
        {
            for (int x = 1; x < image.GetSize().X - 1; x++)
            {
                Color center = image.GetPixel(x, y);
                Color sum = image.GetPixel(x - 1, y) + image.GetPixel(x + 1, y) +
                           image.GetPixel(x, y - 1) + image.GetPixel(x, y + 1);
                
                Color result = center * 5 - sum;
                sharpened.SetPixel(x, y, result);
            }
        }
        
        image.Blend(sharpened);
    }
    
    private void ApplyBrightness(Image image, float amount)
    {
        for (int y = 0; y < image.GetSize().Y; y++)
        {
            for (int x = 0; x < image.GetSize().X; x++)
            {
                Color pixel = image.GetPixel(x, y);
                image.SetPixel(x, y, new Color(
                    Mathf.Clamp(pixel.R + amount, 0, 1),
                    Mathf.Clamp(pixel.G + amount, 0, 1),
                    Mathf.Clamp(pixel.B + amount, 0, 1),
                    pixel.A
                ));
            }
        }
    }
    
    private void ApplyContrast(Image image, float amount)
    {
        float factor = (1 + amount) / (1 - amount);
        
        for (int y = 0; y < image.GetSize().Y; y++)
        {
            for (int x = 0; x < image.GetSize().X; x++)
            {
                Color pixel = image.GetPixel(x, y);
                image.SetPixel(x, y, new Color(
                    Mathf.Clamp((pixel.R - 0.5f) * factor + 0.5f, 0, 1),
                    Mathf.Clamp((pixel.G - 0.5f) * factor + 0.5f, 0, 1),
                    Mathf.Clamp((pixel.B - 0.5f) * factor + 0.5f, 0, 1),
                    pixel.A
                ));
            }
        }
    }
}
