using Godot;

public partial class Layer : RefCounted
{
    private Image _image;
    private string _name;
    private bool _visible = true;
    private float _opacity = 1.0f;
    private int _width;
    private int _height;
    
    public enum BlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        Darken,
        Lighten
    }
    
    private BlendMode _blendMode = BlendMode.Normal;
    
    public Layer(int width, int height, string name = "Layer")
    {
        _width = width;
        _height = height;
        _name = name;
        _image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        _image.Fill(Colors.Transparent);
    }
    
    public Image GetImage() => _image;
    public string Name 
    { 
        get => _name; 
        set => _name = value; 
    }
    
    public bool Visible 
    { 
        get => _visible; 
        set => _visible = value; 
    }
    
    public float Opacity 
    { 
        get => _opacity; 
        set => _opacity = Mathf.Clamp(value, 0.0f, 1.0f); 
    }
    
    public BlendMode Mode 
    { 
        get => _blendMode; 
        set => _blendMode = value; 
    }
    
    public int Width => _width;
    public int Height => _height;
    
    public void Clear()
    {
        _image.Fill(Colors.Transparent);
    }
    
    public void DrawPixel(int x, int y, Color color)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            Color existingColor = _image.GetPixel(x, y);
            Color blendedColor = ApplyBlendMode(existingColor, color, _blendMode);
            blendedColor.A *= _opacity;
            _image.SetPixel(x, y, blendedColor);
        }
    }
    
    public void DrawLine(Vector2 from, Vector2 to, Color color, float width)
    {
        // Bresenham's line algorithm for pixel-perfect lines
        int x0 = (int)from.X, y0 = (int)from.Y;
        int x1 = (int)to.X, y1 = (int)to.Y;
        
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            DrawCircle(new Vector2(x0, y0), width / 2, color);
            
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
    
    public void DrawCircle(Vector2 center, float radius, Color color)
    {
        int cx = (int)center.X;
        int cy = (int)center.Y;
        int r = (int)radius;
        
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < _width && py >= 0 && py < _height)
                    {
                        Color existingColor = _image.GetPixel(px, py);
                        float alpha = color.A * _opacity;
                        Color newColor = new(
                            color.R * alpha + existingColor.R * (1 - alpha),
                            color.G * alpha + existingColor.G * (1 - alpha),
                            color.B * alpha + existingColor.B * (1 - alpha),
                            Mathf.Min(1.0f, alpha + existingColor.A)
                        );
                        _image.SetPixel(px, py, newColor);
                    }
                }
            }
        }
    }
    
    private Color ApplyBlendMode(Color baseColor, Color blendColor, BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Multiply => new(
                baseColor.R * blendColor.R,
                baseColor.G * blendColor.G,
                baseColor.B * blendColor.B,
                Mathf.Max(baseColor.A, blendColor.A)
            ),
            BlendMode.Screen => new(
                1 - (1 - baseColor.R) * (1 - blendColor.R),
                1 - (1 - baseColor.G) * (1 - blendColor.G),
                1 - (1 - baseColor.B) * (1 - blendColor.B),
                Mathf.Max(baseColor.A, blendColor.A)
            ),
            BlendMode.Darken => new(
                Math.Min(baseColor.R, blendColor.R),
                Math.Min(baseColor.G, blendColor.G),
                Math.Min(baseColor.B, blendColor.B),
                Mathf.Max(baseColor.A, blendColor.A)
            ),
            BlendMode.Lighten => new(
                Math.Max(baseColor.R, blendColor.R),
                Math.Max(baseColor.G, blendColor.G),
                Math.Max(baseColor.B, blendColor.B),
                Mathf.Max(baseColor.A, blendColor.A)
            ),
            _ => blendColor
        };
    }
    
    public Image Duplicate()
    {
        return _image.Duplicate() as Image;
    }
}
