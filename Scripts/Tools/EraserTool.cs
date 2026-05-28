using Godot;

namespace PhotoGodot.Tools;

public partial class EraserTool : Core.BaseTool
{
    public override string Name => "Eraser";
    public override string Description => "Erase pixels from the current layer";

    private bool _hasStartedDrawing = false;

    protected override void OnActivate()
    {
        _hasStartedDrawing = false;
    }

    protected override void OnLeftMouseDown(Vector2 position)
    {
        _hasStartedDrawing = false;
        EraseAtPosition(position);
        _hasStartedDrawing = true;
        SaveState("Eraser Stroke", "Erased pixels");
    }

    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_hasStartedDrawing)
        {
            EraseAtPosition(to);
            _hasStartedDrawing = true;
            return;
        }
        
        DrawLine(from, to);
    }

    protected override void OnLeftMouseUp(Vector2 position)
    {
        _hasStartedDrawing = false;
    }

    private void EraseAtPosition(Vector2 position)
    {
        if (WorkingLayer == null) return;
        
        var layerPos = ScreenToLayer(position);
        int x = (int)layerPos.X;
        int y = (int)layerPos.Y;
        float radius = BrushSize / 2;
        
        for (int dy = -(int)Mathf.Ceil(radius); dy <= (int)Mathf.Ceil(radius); dy++)
        {
            for (int dx = -(int)Mathf.Ceil(radius); dx <= (int)Mathf.Ceil(radius); dx++)
            {
                int px = x + dx;
                int py = y + dy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist <= radius)
                {
                    float alpha = Opacity;
                    
                    // Soft eraser falloff
                    if (Hardness < 1.0f && dist > radius * Hardness)
                    {
                        float t = (dist - radius * Hardness) / (radius * (1 - Hardness));
                        alpha *= 1 - t;
                    }
                    
                    var current = WorkingLayer.Image.GetPixel(px, py);
                    float newAlpha = Mathf.Max(0, current.A - alpha);
                    WorkingLayer.Image.SetPixel(px, py, new Color(current.R, current.G, current.B, newAlpha));
                }
            }
        }
        
        WorkingLayer.UpdateTexture();
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        int x0 = (int)from.X;
        int y0 = (int)from.Y;
        int x1 = (int)to.X;
        int y1 = (int)to.Y;
        
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2;
        
        while (true)
        {
            EraseAtPosition(new Vector2(x0, y0));
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }
}
