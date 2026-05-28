using Godot;

namespace PhotoGodot.Tools
{
    /// <summary>
    /// Eraser tool for removing pixels from the canvas
    /// </summary>
    public class EraserTool : BaseTool
    {
        private Vector2 _lastPosition;
        private bool _isErasing;
        
        public override string ToolName => "Eraser";
        public override string ToolDescription => "Erase pixels from canvas";
        public override string ShortcutKey => "3";
        
        public enum EraserMode { Transparent, BackgroundColor }
        public EraserMode CurrentMode { get; set; } = EraserMode.Transparent;
        public Color BackgroundColor { get; set; } = Colors.White;
        
        public override void Activate()
        {
            base.Activate();
            _isErasing = false;
        }
        
        public override void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            
            _isErasing = true;
            _lastPosition = canvasPosition;
            
            LockCanvas();
            EraseStroke(canvasPosition, canvasPosition);
            UnlockCanvas();
            UpdateCanvas();
        }
        
        public override void OnDrag(Vector2 fromPosition, Vector2 toPosition, Vector2 canvasFrom, Vector2 canvasTo)
        {
            if (!IsActive || !_isErasing) return;
            
            LockCanvas();
            EraseStroke(canvasFrom, canvasTo);
            UnlockCanvas();
            UpdateCanvas();
            
            _lastPosition = canvasTo;
        }
        
        public override void OnRelease(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            _isErasing = false;
        }
        
        /// <summary>
        /// Erases a stroke between two points
        /// </summary>
        private void EraseStroke(Vector2 from, Vector2 to)
        {
            float distance = from.DistanceTo(to);
            int steps = Mathf.CeilToInt(distance / (BrushSize * 0.3f));
            
            if (steps == 0)
            {
                EraseStamp(from);
                return;
            }
            
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 pos = from.Lerp(to, t);
                EraseStamp(pos);
            }
        }
        
        /// <summary>
        /// Erases at a single point
        /// </summary>
        private void EraseStamp(Vector2 position)
        {
            int centerX = (int)position.X;
            int centerY = (int)position.Y;
            int radius = (int)(BrushSize / 2f);
            
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        
                        if (pixelX >= 0 && pixelY >= 0 && 
                            pixelX < CanvasImage.GetWidth() && 
                            pixelY < CanvasImage.GetHeight())
                        {
                            switch (CurrentMode)
                            {
                                case EraserMode.Transparent:
                                    Color transparentColor = new Color(0, 0, 0, 0);
                                    CanvasImage.SetPixel(pixelX, pixelY, transparentColor);
                                    break;
                                    
                                case EraserMode.BackgroundColor:
                                    CanvasImage.SetPixel(pixelX, pixelY, BackgroundColor);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        
        public override void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (!IsActive) return;
            
            // Draw eraser preview circle with X
            canvasItem.DrawArc(position, BrushSize / 2f, 0, Mathf.Pi * 2, 32, 
                new Color(1, 1, 1, 0.5f), 2f, true);
                
            // Draw X mark
            float xSize = BrushSize / 3f;
            canvasItem.DrawLine(
                new Vector2(position.X - xSize, position.Y - xSize),
                new Vector2(position.X + xSize, position.Y + xSize),
                Colors.Red, 2f);
            canvasItem.DrawLine(
                new Vector2(position.X + xSize, position.Y - xSize),
                new Vector2(position.X - xSize, position.Y + xSize),
                Colors.Red, 2f);
        }
    }
}
