using Godot;

namespace PhotoGodot.Tools
{
    /// <summary>
    /// Move/Navigation tool for panning around the canvas
    /// </summary>
    public class MoveTool : BaseTool
    {
        [Signal] public delegate void CanvasMovedEventHandler(Vector2 offset);
        
        public override string ToolName => "Move";
        public override string ToolDescription => "Pan and navigate canvas";
        public override string ShortcutKey => "4";
        
        private bool _isPanning;
        private Vector2 _lastPosition;
        
        public float PanSpeed { get; set; } = 1f;
        public bool ConstrainToCanvas { get; set; } = false;
        
        public override void Activate()
        {
            base.Activate();
            _isPanning = false;
        }
        
        public override void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            
            _isPanning = true;
            _lastPosition = position;
        }
        
        public override void OnDrag(Vector2 fromPosition, Vector2 toPosition, Vector2 canvasFrom, Vector2 canvasTo)
        {
            if (!IsActive || !_isPanning) return;
            
            Vector2 delta = toPosition - fromPosition;
            EmitSignal(SignalName.CanvasMoved, delta * PanSpeed);
            
            _lastPosition = toPosition;
        }
        
        public override void OnRelease(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            _isPanning = false;
        }
        
        public override void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (!IsActive) return;
            
            // Draw move cursor (four arrows)
            float arrowSize = 20f;
            Color arrowColor = new Color(1, 1, 1, 0.6f);
            
            // Up arrow
            canvasItem.DrawLine(position, position + Vector2.Up * arrowSize, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Up * arrowSize, 
                position + Vector2.Up * arrowSize + Vector2.Left * 8f, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Up * arrowSize, 
                position + Vector2.Up * arrowSize + Vector2.Right * 8f, arrowColor, 2f);
                
            // Down arrow
            canvasItem.DrawLine(position, position + Vector2.Down * arrowSize, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Down * arrowSize, 
                position + Vector2.Down * arrowSize + Vector2.Left * 8f, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Down * arrowSize, 
                position + Vector2.Down * arrowSize + Vector2.Right * 8f, arrowColor, 2f);
                
            // Left arrow
            canvasItem.DrawLine(position, position + Vector2.Left * arrowSize, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Left * arrowSize, 
                position + Vector2.Left * arrowSize + Vector2.Up * 8f, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Left * arrowSize, 
                position + Vector2.Left * arrowSize + Vector2.Down * 8f, arrowColor, 2f);
                
            // Right arrow
            canvasItem.DrawLine(position, position + Vector2.Right * arrowSize, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Right * arrowSize, 
                position + Vector2.Right * arrowSize + Vector2.Up * 8f, arrowColor, 2f);
            canvasItem.DrawLine(position + Vector2.Right * arrowSize, 
                position + Vector2.Right * arrowSize + Vector2.Down * 8f, arrowColor, 2f);
        }
    }
}
