using Godot;

namespace PhotoGodot.Tools
{
    /// <summary>
    /// Color Picker tool for sampling colors from the canvas
    /// </summary>
    public class ColorPickerTool : BaseTool
    {
        [Signal] public delegate void ColorPickedEventHandler(Color color);
        
        public override string ToolName => "Color Picker";
        public override string ToolDescription => "Pick colors from canvas";
        public override string ShortcutKey => "5";
        
        public bool SampleAllLayers { get; set; } = true;
        public int SampleSize { get; set; } = 1; // 1=point, 3x3, 5x5, etc.
        
        public override void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
            
            Color pickedColor = PickColor(canvasPosition);
            
            if (!pickedColor.Equals(Colors.Transparent))
            {
                EmitSignal(SignalName.ColorPicked, pickedColor);
                GD.Print($"[ColorPicker] Picked color: {pickedColor.ToHtml()}");
            }
        }
        
        /// <summary>
        /// Picks color from canvas at given position
        /// </summary>
        private Color PickColor(Vector2 canvasPosition)
        {
            if (CanvasImage == null) return Colors.Transparent;
            
            int centerX = (int)canvasPosition.X;
            int centerY = (int)canvasPosition.Y;
            
            // Bounds checking
            if (centerX < 0 || centerY < 0 || 
                centerX >= CanvasImage.GetWidth() || 
                centerY >= CanvasImage.GetHeight())
                return Colors.Transparent;
            
            if (SampleSize == 1)
            {
                // Single point sample
                return CanvasImage.GetPixel(centerX, centerY);
            }
            else
            {
                // Area sample (average)
                int halfSize = SampleSize / 2;
                float totalR = 0, totalG = 0, totalB = 0, totalA = 0;
                int count = 0;
                
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
                            Color pixel = CanvasImage.GetPixel(pixelX, pixelY);
                            totalR += pixel.R;
                            totalG += pixel.G;
                            totalB += pixel.B;
                            totalA += pixel.A;
                            count++;
                        }
                    }
                }
                
                return new Color(
                    totalR / count,
                    totalG / count,
                    totalB / count,
                    totalA / count
                );
            }
        }
        
        public override void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (!IsActive) return;
            
            // Draw magnifying glass preview
            float radius = 15f;
            canvasItem.DrawArc(position, radius, 0, Mathf.Pi * 2, 32, 
                new Color(1, 1, 1, 0.7f), 2f, true);
                
            // Draw crosshair
            canvasItem.DrawLine(
                new Vector2(position.X - radius, position.Y),
                new Vector2(position.X + radius, position.Y),
                Colors.White, 1f);
            canvasItem.DrawLine(
                new Vector2(position.X, position.Y - radius),
                new Vector2(position.X, position.Y + radius),
                Colors.White, 1f);
                
            // Show sampled color in center
            if (CanvasImage != null)
            {
                int x = (int)position.X;
                int y = (int)position.Y;
                
                if (x >= 0 && y >= 0 && x < CanvasImage.GetWidth() && y < CanvasImage.GetHeight())
                {
                    Color sampleColor = CanvasImage.GetPixel(x, y);
                    canvasItem.DrawCircle(position, 8f, sampleColor);
                    canvasItem.DrawArc(position, 8f, 0, Mathf.Pi * 2, 16, 
                        Colors.Black, 1f, false);
                }
            }
        }
    }
}
