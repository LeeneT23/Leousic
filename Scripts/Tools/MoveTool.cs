using Godot;

namespace PhotoGodot.Tools;

public partial class MoveTool : Core.BaseTool
{
    public MoveTool()
    {
        ToolName = "Move";
    }

    private Vector2 _dragStartPos = Vector2.Zero;
    private Image _dragSnapshot = null;

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var canvasPos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            
            if (mb.Pressed)
            {
                if (LayerManager.ActiveLayer != null)
                {
                    _dragStartPos = canvasPos;
                    _dragSnapshot = LayerManager.ActiveLayer.GetSnapshot();
                    IsDrawing = true;
                }
            }
            else if (IsDrawing)
            {
                IsDrawing = false;
                _dragSnapshot = null;
                History.SaveState("Mover completado");
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            var canvasPos = MainScene.ScreenToCanvas(mm.GlobalPosition);
            Vector2 delta = canvasPos - _dragStartPos;
            
            // En una implementación completa, esto movería el contenido de la capa
            // Por ahora, solo mostramos feedback visual
            GD.Print($"Moviendo: {delta}");
        }
    }
}
