using Godot;

namespace PhotoGodot.Tools;

public partial class BrushTool : BaseTool
{
    [Export] public float BrushSize { get; set; } = 10.0f;
    [Export] public Color BrushColor { get; set; } = Colors.Black;
    [Export] public float Opacity { get; set; } = 1.0f;
    [Export] public float Hardness { get; set; } = 1.0f;

    public BrushTool()
    {
        ToolName = "Pincel";
        ShortcutKey = "b";
    }

    public override void OnActivate()
    {
        MainScene.SetCursor("cross");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                OnBeginDraw(pos);
                DrawAtPosition(pos);
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                OnEndDraw(pos);
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
                Vector2 offset = new Vector2(x, y);
                if (offset.Length() <= radius)
                {
                    float distFactor = 1.0f - (offset.Length() / radius);
                    float alpha = distFactor < Hardness ? Opacity : Opacity * (1.0f - (distFactor - Hardness) / (1.0f - Hardness));
                    if (Hardness >= 1.0f) alpha = Opacity;
                    
                    LayerManager.ActiveLayer.DrawPixel(pos + offset, BrushColor, alpha);
                }
            }
        }
        CommitChanges();
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        float dist = from.DistanceTo(to);
        int steps = (int)(dist * 2);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 pos = from.Lerp(to, t);
            DrawAtPosition(pos);
        }
    }
}
