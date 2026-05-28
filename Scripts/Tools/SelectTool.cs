using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class SelectTool : BaseTool
{
    private bool _isSelecting = false;
    private Vector2 _startPos;
    private Rect2 _selection;

    public override void OnActivate()
    {
        GD.Print("⬜ Selección activada");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _isSelecting = true;
                _startPos = pos;
                _selection = new Rect2(pos, Vector2.Zero);
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                _isSelecting = false;
                GD.Print($"Selección: {_selection.Size}");
            }
        }
        else if (e is InputEventMouseMotion mm && _isSelecting)
        {
            Vector2 currentPos = MainScene.GetCanvasPosition(mm.Position);
            _selection = new Rect2(_startPos, currentPos - _startPos);
        }
    }
}
