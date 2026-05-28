using Godot;

/// <summary>
/// Herramienta de movimiento para desplazar capas.
/// Permite mover el contenido de la capa activa con el mouse.
/// </summary>
public partial class MoveTool : BaseTool
{
    private Vector2 _dragStartOffset = Vector2.Zero;
    private int? _targetLayerId;
    
    public MoveTool()
    {
        ToolName = "Mover";
        ToolDescription = "Mueve la capa activa o contenido seleccionado";
    }
    
    protected override void OnDrawStart(Vector2 position)
    {
        if (Canvas == null)
            return;
        
        // Obtener la capa activa
        var activeLayer = Canvas.GetLayer(0);
        if (activeLayer != null && !activeLayer.Locked)
        {
            _targetLayerId = activeLayer.Id;
            _dragStartOffset = position - activeLayer.Offset;
        }
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (Canvas == null || !_targetLayerId.HasValue)
            return;
        
        var layer = Canvas.GetLayer(_targetLayerId.Value);
        if (layer != null && !layer.Locked)
        {
            Vector2 newOffset = to - _dragStartOffset;
            Vector2 offsetDelta = newOffset - layer.Offset;
            
            layer.Offset = newOffset;
            Canvas.MarkLayerAsModified(layer.Id);
        }
    }
    
    protected override void OnDrawEnd(Vector2 position)
    {
        _targetLayerId = null;
    }
}
