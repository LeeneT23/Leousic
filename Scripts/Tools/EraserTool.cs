using Godot;
using System.Collections.Generic;

/// <summary>
/// Herramienta de borrador para eliminar contenido de las capas.
/// Funciona como un pincel que restaura la transparencia.
/// </summary>
public partial class EraserTool : BaseTool
{
    private List<Vector2> _currentStroke = new();
    
    public EraserTool()
    {
        ToolName = "Borrador";
        ToolDescription = "Elimina contenido de la capa activa";
        BrushSize = 20.0f;
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
        
        var activeLayer = Canvas.GetLayer(Canvas.GetLayer(0)?.Id ?? -1);
        if (activeLayer != null && activeLayer.Texture != null && !activeLayer.Locked)
        {
            Image img = activeLayer.Texture.GetImage();
            img.Lock();
            
            DrawEraserStroke(img, from, to, BrushSize, Opacity);
            
            img.Unlock();
            activeLayer.Texture.Update(img);
            Canvas.MarkLayerAsModified(activeLayer.Id);
        }
    }
    
    protected override void OnDrawEnd(Vector2 position)
    {
        _currentStroke.Clear();
    }
    
    private void DrawEraserStroke(Image img, Vector2 from, Vector2 to, float size, float opacity)
    {
        int radius = (int)(size / 2);
        int steps = (int)from.DistanceTo(to);
        
        if (steps == 0)
        {
            DrawEraserCircle(img, from, radius, opacity);
            return;
        }
        
        Vector2 direction = (to - from).Normalized();
        
        for (int i = 0; i <= steps; i++)
        {
            Vector2 pos = from + direction * i;
            DrawEraserCircle(img, pos, radius, opacity);
        }
    }
    
    private void DrawEraserCircle(Image img, Vector2 center, int radius, float opacity)
    {
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
                        
                        if (distance > softRadius && hardness < 1.0f)
                        {
                            float t = (distance - softRadius) / (radius - softRadius);
                            alphaMultiplier = 1.0f - t * t;
                        }
                        
                        Color existing = img.GetPixel(px, py);
                        float eraseAmount = opacity * alphaMultiplier;
                        
                        Color erased = new Color(
                            existing.R,
                            existing.G,
                            existing.B,
                            Mathf.Max(0, existing.A - eraseAmount)
                        );
                        
                        img.SetPixel(px, py, erased);
                    }
                }
            }
        }
    }
}
