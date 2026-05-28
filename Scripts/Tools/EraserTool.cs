using Godot;

public partial class EraserTool : BaseTool
{
    public EraserTool()
    {
        _toolName = "Eraser";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        EraseAtPosition(position);
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        float distance = to.DistanceTo(from);
        int steps = Mathf.CeilToInt(distance / 2.0f);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 interpolatedPos = from.Lerp(to, t);
            EraseAtPosition(interpolatedPos);
        }
        
        SaveHistoryState();
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        SaveHistoryState();
    }
    
    private void EraseAtPosition(Vector2 position)
    {
        if (_main.GetLayerManager().ActiveLayer == null) return;
        
        float brushSize = _main.GetBrushSize();
        float opacity = _main.GetOpacity();
        float hardness = _main.GetHardness();
        
        var layer = _main.GetLayerManager().ActiveLayer;
        EraseCircle(layer, position, brushSize, opacity, hardness);
    }
    
    private void EraseCircle(Layer layer, Vector2 center, float size, float opacity, float hardness)
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
                        float eraseAmount;
                        
                        if (distance <= radius * hardness)
                        {
                            eraseAmount = opacity;
                        }
                        else
                        {
                            float fadeRange = radius * (1.0f - hardness);
                            if (fadeRange > 0)
                            {
                                eraseAmount = opacity * (1.0f - (distance - radius * hardness) / fadeRange);
                            }
                            else
                            {
                                eraseAmount = 0;
                            }
                        }
                        
                        Color pixelColor = layer.GetImage().GetPixel(px, py);
                        float newAlpha = Mathf.Max(0, pixelColor.A - eraseAmount);
                        Color erasedColor = new(pixelColor.R, pixelColor.G, pixelColor.B, newAlpha);
                        layer.GetImage().SetPixel(px, py, erasedColor);
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
