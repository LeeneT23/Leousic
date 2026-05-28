using Godot;

namespace PhotoGodot.Tools;

public partial class EraserTool : Core.BaseTool
{
    public EraserTool()
    {
        ToolName = "Eraser";
    }

    private float _size = 20f;

    public override void OnActivate()
    {
        if (MainScene != null)
        {
            _size = MainScene.BrushSize * 2;
        }
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var canvasPos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            
            if (mb.Pressed)
            {
                OnBeginDraw(canvasPos);
                EraseStroke(LastPos, canvasPos, true);
            }
            else if (IsDrawing)
            {
                OnEndDraw(canvasPos);
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            var canvasPos = MainScene.ScreenToCanvas(mm.GlobalPosition);
            EraseStroke(LastPos, canvasPos);
            LastPos = canvasPos;
        }
    }

    private void EraseStroke(Vector2 from, Vector2 to, bool isPoint = false)
    {
        if (LayerManager.ActiveLayer == null) return;

        float dist = from.DistanceTo(to);
        int steps = isPoint ? 1 : (int)(dist / (_size * 0.3f));
        
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0 : (float)i / steps;
            Vector2 pos = from + (to - from) * t;
            
            int radius = (int)(_size / 2);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    float dx = x;
                    float dy = y;
                    float distFromCenter = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (distFromCenter <= radius)
                    {
                        // Borrar = pintar con transparencia
                        LayerManager.ActiveLayer.DrawPixel(pos + new Vector2(x, y), Colors.Transparent);
                    }
                }
            }
        }
        
        CommitChanges();
    }
}
