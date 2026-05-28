using Godot;

namespace PhotoGodot.Tools
{
    /// <summary>
    /// Selection tool for selecting areas of the canvas
    /// </summary>
    public class SelectTool : BaseTool
    {
        [Signal] public delegate void SelectionCreatedEventHandler(Rect2 selection);
        [Signal] public delegate void SelectionClearedEventHandler();
        
        public override string ToolName => "Select";
        public override string ToolDescription => "Create rectangular selections";
        public override string ShortcutKey => "1";
        
        private bool _isSelecting;
        private Vector2 _startPosition;
        private Rect2? _currentSelection;
        
        public enum SelectionMode { Replace, Add, Subtract, Intersect }
        public SelectionMode CurrentMode { get; set; } = SelectionMode.Replace;
        
        public Color SelectionColor { get; set; } = new Color(1, 1, 0, 0.3f);
        public Color BorderColor { get; set; } = Colors.Yellow;
        
        public Rect2? CurrentSelection => _currentSelection;
        
        public override void Activate()
        {
            base.Activate();
            _isSelecting = false;
        }
        
        public override void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            
            _isSelecting = true;
            _startPosition = canvasPosition;
            _currentSelection = null;
        }
        
        public override void OnDrag(Vector2 fromPosition, Vector2 toPosition, Vector2 canvasFrom, Vector2 canvasTo)
        {
            if (!IsActive || !_isSelecting) return;
            
            // Create/update selection rectangle
            Vector2 size = canvasTo - _startPosition;
            _currentSelection = new Rect2(_startPosition, size);
        }
        
        public override void OnRelease(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive || !_isSelecting) return;
            
            _isSelecting = false;
            
            if (_currentSelection.HasValue)
            {
                // Normalize the rectangle (ensure positive width/height)
                Rect2 normalized = _currentSelection.Value.Abs();
                
                if (normalized.Size.X > 5 && normalized.Size.Y > 5) // Minimum size
                {
                    EmitSignal(SignalName.SelectionCreated, normalized);
                    GD.Print($"[Select] Created selection: {normalized}");
                }
                else
                {
                    _currentSelection = null;
                    EmitSignal(SignalName.SelectionCleared);
                }
            }
        }
        
        /// <summary>
        /// Clears the current selection
        /// </summary>
        public void ClearSelection()
        {
            _currentSelection = null;
            EmitSignal(SignalName.SelectionCleared);
        }
        
        /// <summary>
        /// Checks if a point is within the current selection
        /// </summary>
        public bool IsPointInSelection(Vector2 point)
        {
            if (!_currentSelection.HasValue) return false;
            return _currentSelection.Value.HasPoint(point);
        }
        
        public override void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (!IsActive) return;
            
            // Draw selection rectangle if exists
            if (_currentSelection.HasValue)
            {
                Rect2 rect = _currentSelection.Value;
                
                // Fill with semi-transparent color
                canvasItem.DrawColoredRect(rect, SelectionColor);
                
                // Draw border
                canvasItem.DrawRect(rect, false, BorderColor, 2f);
                
                // Draw corner handles
                float handleSize = 8f;
                Color handleColor = Colors.White;
                
                // Top-left
                canvasItem.DrawRect(new Rect2(rect.Position.X - handleSize/2, rect.Position.Y - handleSize/2, 
                    handleSize, handleSize), true, handleColor);
                // Top-right
                canvasItem.DrawRect(new Rect2(rect.End.X - handleSize/2, rect.Position.Y - handleSize/2, 
                    handleSize, handleSize), true, handleColor);
                // Bottom-left
                canvasItem.DrawRect(new Rect2(rect.Position.X - handleSize/2, rect.End.Y - handleSize/2, 
                    handleSize, handleSize), true, handleColor);
                // Bottom-right
                canvasItem.DrawRect(new Rect2(rect.End.X - handleSize/2, rect.End.Y - handleSize/2, 
                    handleSize, handleSize), true, handleColor);
                    
                // Display selection info
                string info = $"W: {(int)rect.Size.X} H: {(int)rect.Size.Y}";
                Vector2 textPos = new Vector2(rect.Position.X, rect.Position.Y - 20);
                // Note: Actual text rendering would require a Label or custom font drawing
            }
        }
    }
}
