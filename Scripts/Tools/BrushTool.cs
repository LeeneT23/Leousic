using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Herramienta de pincel para dibujar trazos libres.
/// Soporta presión, opacidad, dureza y tamaños variables.
/// </summary>
public partial class BrushTool : BaseTool
{
    private List<Vector2> _currentStroke = new();
    
    public BrushTool()
    {
        ToolName = "Pincel";
        ToolDescription = "Dibuja trazos libres con el color seleccionado";
        BrushSize = 10.0f;
        Opacity = 1.0f;
    }
    
    protected override void OnDrawStart(Vector2 position)
    {
        _currentStroke.Clear();
        _currentStroke.Add(position);
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (Canvas == null)
            return;
        
        _currentStroke.Add(to);
        
        // Dibujar en tiempo real (preview)
        var activeLayer = Canvas.GetLayer(Canvas.GetLayer(0)?.Id ?? -1);
        if (activeLayer != null && activeLayer.Texture != null && !activeLayer.Locked)
        {
            Image img = activeLayer.Texture.GetImage();
            img.Lock();
            
            DrawBrushStroke(img, from, to, PrimaryColor, BrushSize, Opacity);
            
            img.Unlock();
            activeLayer.Texture.Update(img);
            Canvas.MarkLayerAsModified(activeLayer.Id);
        }
    }
    
    protected override void OnDrawEnd(Vector2 position)
    {
        // El trazo ya fue dibujado en tiempo real
        _currentStroke.Clear();
    }
    
    private void DrawBrushStroke(Image img, Vector2 from, Vector2 to, Color color, float size, float opacity)
    {
        int radius = (int)(size / 2);
        int steps = (int)from.DistanceTo(to);
        
        if (steps == 0)
        {
            DrawBrushCircle(img, from, radius, color, opacity);
            return;
        }
        
        Vector2 direction = (to - from).Normalized();
        
        for (int i = 0; i <= steps; i++)
        {
            Vector2 pos = from + direction * i;
            DrawBrushCircle(img, pos, radius, color, opacity);
        }
    }
    
    private void DrawBrushCircle(Image img, Vector2 center, int radius, Color color, float opacity)
    {
        Color colorWithOpacity = new Color(color.R, color.G, color.B, color.A * opacity);
        
        // Hardness determina qué tan suave es el borde
        float hardness = BrushHardness;
        int softRadius = (int)(radius * (1 - hardness));
        
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                float distance = Mathf.Sqrt(x * x + y * y);
                
                if (distance <= radius)
                {
                    int px = (int)(center.X + x);
                    int py = (int)(center.Y + y);
                    
                    if (px >= 0 && px < img.GetSize().X && py >= 0 && py < img.GetSize().Y)
                    {
                        float alphaMultiplier = 1.0f;
                        
                        // Aplicar suavizado basado en hardness
                        if (distance > softRadius && hardness < 1.0f)
                        {
                            float t = (distance - softRadius) / (radius - softRadius);
                            alphaMultiplier = 1.0f - t * t; // Curva suave
                        }
                        
                        Color finalColor = new Color(
                            colorWithOpacity.R,
                            colorWithOpacity.G,
                            colorWithOpacity.B,
                            colorWithOpacity.A * alphaMultiplier
                        );
                        
                        Color existing = img.GetPixel(px, py);
                        Color blended = BlendColors(existing, finalColor);
                        img.SetPixel(px, py, blended);
                    }
                }
            }
        }
    }
    
    private Color BlendColors(Color background, Color foreground)
    {
        float alpha = foreground.A;
        return new Color(
            background.R * (1 - alpha) + foreground.R * alpha,
            background.G * (1 - alpha) + foreground.G * alpha,
            background.B * (1 - alpha) + foreground.B * alpha,
            Mathf.Max(background.A, foreground.A)
        );
    }
    
    public override Dictionary<string, Variant> GetToolSettings()
    {
        var settings = base.GetToolSettings();
        settings["PrimaryColor"] = PrimaryColor;
        return settings;
    }
    
    public override void ApplyToolSettings(Dictionary<string, Variant> settings)
    {
        base.ApplyToolSettings(settings);
        
        if (settings.TryGetValue("PrimaryColor", out var color))
            PrimaryColor = color.AsColor();
    }
}
