using Godot;
using System;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class Layer : Resource
{
    [Export] public string LayerName { get; set; } = "Nueva Capa";
    [Export] public bool IsVisible { get; set; } = true;
    [Export] public float Opacity { get; set; } = 1.0f;
    [Export] public int BlendModeIndex { get; set; } = 0; // 0=Mix, 1=Add, 2=Subtract, etc.
    
    private Image _image;
    private Texture2D _texture;

    public Image ImageData => _image;
    public Texture2D Texture => _texture;
    public int Width => _image?.GetWidth() ?? 0;
    public int Height => _image?.GetHeight() ?? 0;

    public void Initialize(int width, int height, Color clearColor)
    {
        _image = Image.Create(width, height, false, Image.Format.Rgba8);
        _image.Fill(clearColor);
        _texture = ImageTexture.CreateFromImage(_image);
    }

    public void DrawPixel(Vector2 pos, Color color)
    {
        if (_image == null || !IsVisible) return;
        int x = (int)pos.X;
        int y = (int)pos.Y;
        
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            Color current = _image.GetPixel(x, y);
            
            // Mezcla Alpha estándar (Alpha Over)
            float a = color.A + current.A * (1 - color.A);
            if (a == 0) return;
            
            Vector3 rgb = (color.Rgb * color.A + current.Rgb * current.A * (1 - color.A)) / a;
            _image.SetPixel(x, y, new Color(rgb.X, rgb.Y, rgb.Z, a));
        }
    }

    public void DrawPixelWithOpacity(Vector2 pos, Color color, float opacity)
    {
        color.A *= opacity;
        DrawPixel(pos, color);
    }

    public void UpdateTexture()
    {
        if (_image != null && _texture != null)
        {
            _texture.Update(_image);
        }
    }

    public Color GetPixel(Vector2 pos)
    {
        if (_image == null) return Colors.Transparent;
        int x = (int)pos.X;
        int y = (int)pos.Y;
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            return _image.GetPixel(x, y);
        return Colors.Transparent;
    }
    
    public Image GetSnapshot()
    {
        return _image?.Duplicate();
    }
    
    public void RestoreSnapshot(Image snapshot)
    {
        if (snapshot == null) return;
        _image = snapshot.Duplicate();
        UpdateTexture();
    }
    
    public void Fill(Color color)
    {
        if (_image == null) return;
        _image.Fill(color);
        UpdateTexture();
    }
    
    public void Clear()
    {
        Fill(Colors.Transparent);
    }
}
