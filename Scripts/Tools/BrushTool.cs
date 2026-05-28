using Godot;
using System;

namespace PhotoGodot.Tools;

public partial class BrushTool : Core.BaseTool
{
    public override string Name => "Brush";
    public override string Description => "Draw with customizable brush";
    
    private bool _hasStartedDrawing = false;

    protected override void OnActivate()
    {
        _hasStartedDrawing = false;
    }

    protected override void OnLeftMouseDown(Vector2 position)
    {
        _hasStartedDrawing = false;
        DrawAtPosition(position);
        _hasStartedDrawing = true;
        SaveState("Brush Stroke", "Drew with brush");
    }

    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_hasStartedDrawing)
        {
            DrawAtPosition(to);
            _hasStartedDrawing = true;
            return;
        }
        
        // Bresenham's line algorithm for smooth strokes
        DrawLine(from, to);
    }

    protected override void OnLeftMouseUp(Vector2 position)
    {
        _hasStartedDrawing = false;
    }

    private void DrawAtPosition(Vector2 position)
    {
        if (WorkingLayer == null) return;
        
        var layerPos = ScreenToLayer(position);
        int x = (int)layerPos.X;
        int y = (int)layerPos.Y;
        float radius = BrushSize / 2;
        
        if (Hardness >= 0.95f)
        {
            // Hard brush - fill circle
            for (int dy = -(int)Mathf.Ceil(radius); dy <= (int)Mathf.Ceil(radius); dy++)
            {
                for (int dx = -(int)Mathf.Ceil(radius); dx <= (int)Mathf.Ceil(radius); dx++)
                {
                    int px = x + dx;
                    int py = y + dy;
                    
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        WorkingLayer.DrawPixel(px, py, PrimaryColor, Opacity);
                    }
                }
            }
        }
        else
        {
            // Soft brush - gradient falloff
            float softnessRadius = radius * (1 - Hardness);
            
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
                        
                        if (dist > softnessRadius)
                        {
                            // Falloff zone
                            float t = (dist - softnessRadius) / (radius - softnessRadius);
                            alpha *= 1 - t;
                        }
                        else if (Hardness > 0)
                        {
                            // Partial hardness
                            float t = dist / softnessRadius;
                            alpha *= 1 - t * (1 - Hardness);
                        }
                        
                        WorkingLayer.DrawPixel(px, py, PrimaryColor, alpha);
                    }
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
            DrawAtPosition(new Vector2(x0, y0));
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }
}
