using Godot;

namespace PhotoGodot.Tools
{
    /// <summary>
    /// Brush tool for painting on the canvas
    /// </summary>
    public class BrushTool : BaseTool
    {
        private Vector2 _lastPosition;
        private bool _isDrawing;
        
        public override string ToolName => "Brush";
        public override string ToolDescription => "Paint with customizable brush";
        public override string ShortcutKey => "2";
        
        // Brush-specific properties
        public float Flow { get; set; } = 1f;
        public bool PressureSensitive { get; set; } = true;
        public enum BrushShape { Circle, Square, Soft }
        public BrushShape CurrentShape { get; set; } = BrushShape.Circle;
        
        public override void Activate()
        {
            base.Activate();
            _isDrawing = false;
        }
        
        public override void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            
            _isDrawing = true;
            _lastPosition = canvasPosition;
            
            LockCanvas();
            DrawStroke(canvasPosition, canvasPosition);
            UnlockCanvas();
            UpdateCanvas();
        }
        
        public override void OnDrag(Vector2 fromPosition, Vector2 toPosition, Vector2 canvasFrom, Vector2 canvasTo)
        {
            if (!IsActive || !_isDrawing) return;
            
            LockCanvas();
            DrawStroke(canvasFrom, canvasTo);
            UnlockCanvas();
            UpdateCanvas();
            
            _lastPosition = canvasTo;
        }
        
        public override void OnRelease(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            _isDrawing = false;
        }
        
        /// <summary>
        /// Draws a stroke between two points with interpolation
        /// </summary>
        private void DrawStroke(Vector2 from, Vector2 to)
        {
            float distance = from.DistanceTo(to);
            int steps = Mathf.CeilToInt(distance / (BrushSize * 0.3f));
            
            if (steps == 0)
            {
                DrawStamp(from);
                return;
            }
            
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 pos = from.Lerp(to, t);
                DrawStamp(pos);
            }
        }
        
        /// <summary>
        /// Draws a single brush stamp at the given position
        /// </summary>
        private void DrawStamp(Vector2 position)
        {
            Color brushColor = new Color(BrushColor.R, BrushColor.G, BrushColor.B, BrushColor.A * BrushOpacity * Flow);
            
            switch (CurrentShape)
            {
                case BrushShape.Circle:
                    DrawCircleOnCanvas(position, BrushSize / 2f, brushColor);
                    break;
                    
                case BrushShape.Square:
                    DrawSquareOnCanvas(position, BrushSize, brushColor);
                    break;
                    
                case BrushShape.Soft:
                    DrawSoftBrushOnCanvas(position, BrushSize / 2f, brushColor);
                    break;
            }
        }
        
        /// <summary>
        /// Draws a square brush stamp
        /// </summary>
        private void DrawSquareOnCanvas(Vector2 center, float size, Color color)
        {
            if (CanvasImage == null) return;
            
            int halfSize = (int)(size / 2f);
            int centerX = (int)center.X;
            int centerY = (int)center.Y;
            
            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    
                    if (pixelX >= 0 && pixelY >= 0 && 
                        pixelX < CanvasImage.GetWidth() && 
                        pixelY < CanvasImage.GetHeight())
                    {
                        Color existingColor = CanvasImage.GetPixel(pixelX, pixelY);
                        Color blendedColor = BlendColors(existingColor, color);
                        CanvasImage.SetPixel(pixelX, pixelY, blendedColor);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws a soft brush with gradient falloff
        /// </summary>
        private void DrawSoftBrushOnCanvas(Vector2 center, float radius, Color color)
        {
            if (CanvasImage == null) return;
            
            int centerX = (int)center.X;
            int centerY = (int)center.Y;
            int r = (int)radius;
            
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    float distance = Mathf.Sqrt(x * x + y * y);
                    if (distance <= r)
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        
                        if (pixelX >= 0 && pixelY >= 0 && 
                            pixelX < CanvasImage.GetWidth() && 
                            pixelY < CanvasImage.GetHeight())
                        {
                            // Calculate falloff (softer at edges)
                            float falloff = 1f - (distance / r);
                            falloff = falloff * falloff; // Quadratic falloff for smoother edges
                            
                            Color softColor = new Color(color.R, color.G, color.B, color.A * falloff);
                            Color existingColor = CanvasImage.GetPixel(pixelX, pixelY);
                            Color blendedColor = BlendColors(existingColor, softColor);
                            CanvasImage.SetPixel(pixelX, pixelY, blendedColor);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Blends two colors using alpha blending
        /// </summary>
        private Color BlendColors(Color background, Color foreground)
        {
            return new Color(
                background.R + (foreground.R - background.R) * foreground.A,
                background.G + (foreground.G - background.G) * foreground.A,
                background.B + (foreground.B - background.B) * foreground.A,
                Mathf.Min(background.A + foreground.A, 1f)
            );
        }
        
        public override void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (!IsActive) return;
            
            // Draw brush preview circle
            canvasItem.DrawArc(position, BrushSize / 2f, 0, Mathf.Pi * 2, 32, 
                new Color(1, 1, 1, 0.5f), 2f, true);
                
            // Draw crosshair
            float crosshairSize = BrushSize / 2f + 10;
            canvasItem.DrawLine(
                new Vector2(position.X - crosshairSize, position.Y),
                new Vector2(position.X + crosshairSize, position.Y),
                Colors.White, 1f);
            canvasItem.DrawLine(
                new Vector2(position.X, position.Y - crosshairSize),
                new Vector2(position.X, position.Y + crosshairSize),
                Colors.White, 1f);
        }
    }
}
