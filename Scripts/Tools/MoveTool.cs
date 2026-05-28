using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class MoveTool : BaseTool
{
    private bool _isMoving = false;
    private Vector2 _startPos;

    public override void OnActivate()
    {
        GD.Print("✋ Herramienta Mover activada");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                _isMoving = true;
                _startPos = pos;
                History.SaveState("Mover");
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                _isMoving = false;
            }
        }
        else if (e is InputEventMouseMotion mm && _isMoving)
        {
            Vector2 currentPos = MainScene.GetCanvasPosition(mm.Position);
            Vector2 delta = currentPos - _startPos;
            
            // Aquí se implementaría el movimiento real de la capa
            // Por simplicidad, solo actualizamos la posición inicial
            _startPos = currentPos;
        }
    }
}
