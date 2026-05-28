using Godot;

namespace PhotoGodot.Tools;

public partial class SelectTool : BaseTool
{
    private Vector2 _startPos;
    private Rect2 _selection;

    public SelectTool()
    {
        ToolName = "Seleccionar";
        ShortcutKey = "m";
    }

    public override void OnActivate()
    {
        MainScene.SetCursor("cross");
        MainScene.ShowSelection(true);
    }

    public override void OnDeactivate()
    {
        MainScene.ShowSelection(false);
        _selection = new Rect2();
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _startPos = pos;
                IsDrawing = true;
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                IsDrawing = false;
                MainScene.UpdateSelection(_selection);
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            Vector2 currentPos = MainScene.GetCanvasPosition(mm.Position);
            _selection = new Rect2(_startPos, currentPos - _startPos);
            MainScene.UpdateSelection(_selection);
        }
    }
}
