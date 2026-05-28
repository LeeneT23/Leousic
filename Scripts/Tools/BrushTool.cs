using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class BrushTool : BaseTool
{
    [Export] public float BrushSize { get; set; } = 10.0f;
    [Export] public float BrushHardness { get; set; } = 1.0f;
    [Export] public float Opacity { get; set; } = 1.0f;
    
    private Color _currentColor;

    public override void OnActivate()
    {
        GD.Print("🖌️ Pincel activado");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _currentColor = MainScene.PrimaryColor;
                OnBeginDraw(pos);
                DrawAtPosition(pos);
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                OnEndDraw(pos);
            }
            
            if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                _currentColor = MainScene.SecondaryColor;
                OnBeginDraw(pos);
                DrawAtPosition(pos);
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mm.Position);
            DrawLine(LastPos, pos);
            LastPos = pos;
        }
    }

    private void DrawAtPosition(Vector2 pos)
    {
        if (LayerManager.ActiveLayer == null) return;
        
        int radius = (int)(BrushSize / 2);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2 offset = new(x, y);
                if (offset.Length() <= radius)
                {
                    float alpha = 1.0f;
                    if (BrushHardness < 1.0f)
                    {
                        float dist = offset.Length() / radius;
                        alpha = Math.Max(0, 1.0f - dist * (1.0f - BrushHardness));
                    }
                    
                    LayerManager.ActiveLayer.DrawPixelWithAlpha(
                        pos + offset, 
                        _currentColor, 
                        alpha * Opacity
                    );
                }
            }
        }
        CommitChanges();
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        float dist = from.DistanceTo(to);
        int steps = (int)Math.Ceil(dist * 2);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 pos = from.Lerp(to, t);
            DrawAtPosition(pos);
        }
    }
}
