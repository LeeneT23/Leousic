using Godot;

public partial class BrushTool : BaseTool
{
    public BrushTool()
    {
        _toolName = "Brush";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        DrawAtPosition(position);
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Interpolate between points for smooth lines
        float distance = to.DistanceTo(from);
        int steps = Mathf.CeilToInt(distance / 2.0f);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 interpolatedPos = from.Lerp(to, t);
            DrawAtPosition(interpolatedPos);
        }
        
        SaveHistoryState();
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        SaveHistoryState();
    }
    
    private void DrawAtPosition(Vector2 position)
    {
        if (_main.GetLayerManager().ActiveLayer == null) return;
        
        float brushSize = _main.GetBrushSize();
        Color color = _main.GetPrimaryColor();
        float opacity = _main.GetOpacity();
        float hardness = _main.GetHardness();
        
        // Apply opacity to color
        Color colorWithOpacity = new(color.R, color.G, color.B, opacity);
        
        var layer = _main.GetLayerManager().ActiveLayer;
        
        if (hardness >= 0.95f)
        {
            // Hard brush - solid circle
            layer.DrawCircle(position, brushSize / 2, colorWithOpacity);
        }
        else
        {
            // Soft brush - gradient effect
            DrawSoftBrush(layer, position, brushSize, colorWithOpacity, hardness);
        }
    }
    
    private void DrawSoftBrush(Layer layer, Vector2 center, float size, Color color, float hardness)
    {
        int radius = (int)(size / 2);
        int cx = (int)center.X;
        int cy = (int)center.Y;
        
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                float distance = Mathf.Sqrt(x * x + y * y);
                if (distance <= radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    
                    if (px >= 0 && px < layer.Width && py >= 0 && py < layer.Height)
                    {
                        // Calculate alpha based on distance and hardness
                        float normalizedDistance = distance / radius;
                        float alpha;
                        
                        if (normalizedDistance <= hardness)
                        {
                            alpha = color.A;
                        }
                        else
                        {
                            // Fade out from hardness edge to full radius
                            float fadeRange = 1.0f - hardness;
                            if (fadeRange > 0)
                            {
                                alpha = color.A * (1.0f - (normalizedDistance - hardness) / fadeRange);
                            }
                            else
                            {
                                alpha = 0;
                            }
                        }
                        
                        Color pixelColor = new(color.R, color.G, color.B, alpha);
                        layer.DrawPixel(px, py, pixelColor);
                    }
                }
            }
        }
    }
    
    private void SaveHistoryState()
    {
        var compositedImage = _main.GetLayerManager().GetCompositedImage();
        if (compositedImage != null)
        {
            _main.GetHistoryManager().SaveState(compositedImage);
        }
    }
}
