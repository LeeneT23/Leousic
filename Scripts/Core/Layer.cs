using Godot;
using System;

namespace PhotoGodot.Core;

public partial class Layer : RefCounted
{
    public string Name { get; set; } = "Layer";
    public Image Image { get; private set; }
    public bool Visible { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public Texture2D Texture { get; private set; }
    public CanvasBlendMode BlendMode { get; set; } = CanvasBlendMode.Mix;
    
    public int Width => Image?.GetWidth() ?? 0;
    public int Height => Image?.GetHeight() ?? 0;

    public enum CanvasBlendMode
    {
        Mix,
        Add,
        Subtract,
        Multiply,
        Screen,
        Overlay
    }

    public Layer(int width, int height, string name = "Layer")
    {
        Name = name;
        Image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        Image.Fill(Colors.Transparent);
        UpdateTexture();
    }

    public Layer(Image image, string name = "Layer")
    {
        Name = name;
        Image = image;
        UpdateTexture();
    }

    public void UpdateTexture()
    {
        if (Image != null)
        {
            var imgCopy = Image.Duplicate();
            
            if (Opacity < 1.0f)
            {
                for (int y = 0; y < imgCopy.GetHeight(); y++)
                {
                    for (int x = 0; x < imgCopy.GetWidth(); x++)
                    {
                        var pixel = imgCopy.GetPixel(x, y);
                        pixel.A *= Opacity;
                        imgCopy.SetPixel(x, y, pixel);
                    }
                }
            }
            
            Texture = ImageTexture.CreateFromImage(imgCopy);
        }
    }

    public void DrawPixel(int x, int y, Color color, float alpha = 1.0f)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        
        var current = Image.GetPixel(x, y);
        var blended = BlendColors(current, color * alpha, alpha);
        Image.SetPixel(x, y, blended);
    }

    public void DrawLine(Vector2 from, Vector2 to, Color color, float width, float alpha = 1.0f)
    {
        Image.DrawLine(from, to, color * new Color(1, 1, 1, alpha), (int)Mathf.Ceil(width));
    }

    public void DrawCircle(Vector2 center, float radius, Color color, bool filled = true, float alpha = 1.0f)
    {
        if (filled)
        {
            Image.FillCircle(center, (int)radius, color * new Color(1, 1, 1, alpha));
        }
        else
        {
            // Draw circle outline
            for (float angle = 0; angle < Mathf.Tau; angle += 0.05f)
            {
                var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Image.SetPixel((int)point.X, (int)point.Y, color * new Color(1, 1, 1, alpha));
            }
        }
    }

    public void FillRect(Rect2 rect, Color color, float alpha = 1.0f)
    {
        for (int y = (int)rect.Position.Y; y < rect.End.Y && y < Height; y++)
        {
            for (int x = (int)rect.Position.X; x < rect.End.X && x < Width; x++)
            {
                if (x >= 0 && y >= 0)
                {
                    DrawPixel(x, y, color, alpha);
                }
            }
        }
    }

    public void Clear()
    {
        Image.Fill(Colors.Transparent);
        UpdateTexture();
    }

    public void ApplyFilter(Func<Color, Color> filterFunc)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var pixel = Image.GetPixel(x, y);
                Image.SetPixel(x, y, filterFunc(pixel));
            }
        }
        UpdateTexture();
    }

    public void ApplyBlur(int radius = 2)
    {
        var blurred = Image.Duplicate();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Color sum = Colors.Transparent;
                int count = 0;
                
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        
                        if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                        {
                            sum += Image.GetPixel(nx, ny);
                            count++;
                        }
                    }
                }
                
                if (count > 0)
                {
                    blurred.SetPixel(x, y, sum / count);
                }
            }
        }
        Image = blurred;
        UpdateTexture();
    }

    public void ApplySharpen()
    {
        var sharpened = Image.Duplicate();
        float[,] kernel = {
            { 0, -1, 0 },
            { -1, 5, -1 },
            { 0, -1, 0 }
        };
        
        for (int y = 1; y < Height - 1; y++)
        {
            for (int x = 1; x < Width - 1; x++)
            {
                Color sum = Colors.Black;
                
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        var pixel = Image.GetPixel(x + kx, y + ky);
                        sum += pixel * kernel[ky + 1, kx + 1];
                    }
                }
                
                sharpened.SetPixel(x, y, sum);
            }
        }
        Image = sharpened;
        UpdateTexture();
    }

    public void Grayscale()
    {
        ApplyFilter(c =>
        {
            float gray = c.R * 0.299f + c.G * 0.587f + c.B * 0.114f;
            return new Color(gray, gray, gray, c.A);
        });
    }

    public void Invert()
    {
        ApplyFilter(c => new Color(1 - c.R, 1 - c.G, 1 - c.B, c.A));
    }

    public void AdjustBrightness(float amount)
    {
        ApplyFilter(c =>
        {
            return new Color(
                Mathf.Clamp(c.R + amount, 0, 1),
                Mathf.Clamp(c.G + amount, 0, 1),
                Mathf.Clamp(c.B + amount, 0, 1),
                c.A
            );
        });
    }

    public void AdjustContrast(float amount)
    {
        float factor = (1 + amount) / (1 - amount);
        ApplyFilter(c =>
        {
            return new Color(
                Mathf.Clamp(factor * (c.R - 0.5f) + 0.5f, 0, 1),
                Mathf.Clamp(factor * (c.G - 0.5f) + 0.5f, 0, 1),
                Mathf.Clamp(factor * (c.B - 0.5f) + 0.5f, 0, 1),
                c.A
            );
        });
    }

    public Image GetCompositedImage(Layer[] layersBelow)
    {
        var result = Image.Duplicate();
        
        foreach (var layer in layersBelow)
        {
            if (!layer.Visible) continue;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var below = layer.Image.GetPixel(x, y);
                    var above = result.GetPixel(x, y);
                    result.SetPixel(x, y, CompositePixels(below, above, layer.BlendMode));
                }
            }
        }
        
        return result;
    }

    private Color BlendColors(Color bg, Color fg, float alpha)
    {
        return new Color(
            bg.R * (1 - alpha) + fg.R * alpha,
            bg.G * (1 - alpha) + fg.G * alpha,
            bg.B * (1 - alpha) + fg.B * alpha,
            Mathf.Max(bg.A, fg.A * alpha)
        );
    }

    private Color CompositePixels(Color below, Color above, CanvasBlendMode mode)
    {
        float a = above.A;
        float b = below.A;
        float outA = a + b * (1 - a);
        
        if (outA == 0) return Colors.Transparent;
        
        Color outRGB;
        
        switch (mode)
        {
            case CanvasBlendMode.Add:
                outRGB = new Color(
                    Mathf.Min(above.R * a + below.R * b, 1),
                    Mathf.Min(above.G * a + below.G * b, 1),
                    Mathf.Min(above.B * a + below.B * b, 1)
                );
                break;
            case CanvasBlendMode.Subtract:
                outRGB = new Color(
                    Mathf.Max(above.R * a - below.R * b, 0),
                    Mathf.Max(above.G * a - below.G * b, 0),
                    Mathf.Max(above.B * a - below.B * b, 0)
                );
                break;
            case CanvasBlendMode.Multiply:
                outRGB = new Color(
                    above.R * below.R + below.R * (1 - a) + above.R * (1 - b),
                    above.G * below.G + below.G * (1 - a) + above.G * (1 - b),
                    above.B * below.B + below.B * (1 - a) + above.B * (1 - b)
                );
                break;
            case CanvasBlendMode.Screen:
                outRGB = new Color(
                    1 - (1 - above.R) * (1 - below.R),
                    1 - (1 - above.G) * (1 - below.G),
                    1 - (1 - above.B) * (1 - below.B)
                );
                break;
            case CanvasBlendMode.Overlay:
                outRGB = new Color(
                    below.R < 0.5 ? 2 * above.R * below.R : 1 - 2 * (1 - above.R) * (1 - below.R),
                    below.G < 0.5 ? 2 * above.G * below.G : 1 - 2 * (1 - above.G) * (1 - below.G),
                    below.B < 0.5 ? 2 * above.B * below.B : 1 - 2 * (1 - above.B) * (1 - below.B)
                );
                break;
            default: // Mix
                outRGB = new Color(
                    (above.R * a + below.R * b * (1 - a)) / outA,
                    (above.G * a + below.G * b * (1 - a)) / outA,
                    (above.B * a + below.B * b * (1 - a)) / outA
                );
                break;
        }
        
        return new Color(outRGB.R, outRGB.G, outRGB.B, outA);
    }

    public byte[] SaveToBytes()
    {
        return Image.SavePngToBuffer();
    }

    public static Layer LoadFromBytes(byte[] data, int width, int height, string name = "Layer")
    {
        var img = Image.New();
        img.LoadPngFromBuffer(data);
        return new Layer(img, name);
    }
}
