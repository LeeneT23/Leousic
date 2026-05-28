using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class EraserTool : BaseTool
{
    [Export] public float EraserSize { get; set; } = 15.0f;
    [Export] public float Hardness { get; set; } = 0.5f;

    public override void OnActivate()
    {
        GD.Print("🧼 Borrador activado");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                OnBeginDraw(pos);
                EraseAtPosition(pos);
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                OnEndDraw(pos);
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mm.Position);
            EraseLine(LastPos, pos);
            LastPos = pos;
        }
    }

    private void EraseAtPosition(Vector2 pos)
    {
        if (LayerManager.ActiveLayer == null) return;
        
        int radius = (int)(EraserSize / 2);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2 offset = new(x, y);
                if (offset.Length() <= radius)
                {
                    float alpha = 1.0f;
                    if (Hardness < 1.0f)
                    {
                        float dist = offset.Length() / radius;
                        alpha = Math.Max(0, 1.0f - dist * (1.0f - Hardness));
                    }
                    
                    var currentPos = pos + offset;
                    var current = LayerManager.ActiveLayer.ImageData.GetPixel((int)currentPos.X, (int)currentPos.Y);
                    current.A *= (1.0f - alpha);
                    LayerManager.ActiveLayer.ImageData.SetPixel((int)currentPos.X, (int)currentPos.Y, current);
                }
            }
        }
        CommitChanges();
    }

    private void EraseLine(Vector2 from, Vector2 to)
    {
        float dist = from.DistanceTo(to);
        int steps = (int)Math.Ceil(dist * 2);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 pos = from.Lerp(to, t);
            EraseAtPosition(pos);
        }
    }
}
