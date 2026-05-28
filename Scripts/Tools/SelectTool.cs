using Godot;

namespace PhotoGodot.Tools;

public partial class SelectTool : Core.BaseTool
{
    public SelectTool()
    {
        ToolName = "Select";
    }

    private Vector2 _selectionStart = Vector2.Zero;
    private Rect2 _currentSelection = Rect2.Zero;
    private bool _isSelecting = false;

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var canvasPos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            
            if (mb.Pressed)
            {
                _selectionStart = canvasPos;
                _isSelecting = true;
                _currentSelection = new Rect2(canvasPos, Vector2.Zero);
            }
            else if (_isSelecting)
            {
                _isSelecting = false;
                GD.Print($"Selección creada: {_currentSelection}");
                History.SaveState("Selección completada");
            }
        }
        else if (e is InputEventMouseMotion mm && _isSelecting)
        {
            var canvasPos = MainScene.ScreenToCanvas(mm.GlobalPosition);
            Vector2 size = canvasPos - _selectionStart;
            _currentSelection = new Rect2(_selectionStart, size).Abs();
        }
    }
}
