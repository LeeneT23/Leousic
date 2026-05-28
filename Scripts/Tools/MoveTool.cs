using Godot;

namespace PhotoGodot.Tools;

public partial class MoveTool : BaseTool
{
    private Vector2 _offset;
    private bool _isMoving = false;

    public MoveTool()
    {
        ToolName = "Mover";
        ShortcutKey = "v";
    }

    public override void OnActivate()
    {
        MainScene.SetCursor("arrow");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _isMoving = true;
                _offset = pos;
                History.SaveState("Mover Inicio");
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                _isMoving = false;
            }
        }
        else if (e is InputEventMouseMotion mm && _isMoving)
        {
            Vector2 newPos = MainScene.GetCanvasPosition(mm.Position);
            Vector2 delta = newPos - _offset;
            // En una implementación completa, aquí se movería el contenido de la capa
            _offset = newPos;
        }
    }
}
