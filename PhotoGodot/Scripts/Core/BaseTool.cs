using Godot;
using System;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Base class for all tools in PhotoGodot.
    /// Provides a common interface and shared functionality.
    /// </summary>
    public abstract class BaseTool : Node
    {
        [Signal] public delegate void ToolActivatedEventHandler();
        [Signal] public delegate void ToolDeactivatedEventHandler();
        
        public string ToolName { get; protected set; } = "Base Tool";
        public string ToolDescription { get; protected set; } = "Base tool description";
        public string ShortcutKey { get; protected set; } = "";
        public bool IsActive { get; protected set; } = false;
        
        // Canvas references
        protected ImageTexture CanvasTexture { get; private set; }
        protected Image CanvasImage { get; private set; }
        protected Control CanvasControl { get; private set; }
        
        // Tool properties (can be overridden)
        public virtual float BrushSize { get; set; } = 20f;
        public virtual float BrushHardness { get; set; } = 1f;
        public virtual float BrushOpacity { get; set; } = 1f;
        public virtual Color BrushColor { get; set; } = Colors.White;
        
        public virtual void Initialize(ImageTexture texture, Image image, Control canvas)
        {
            CanvasTexture = texture;
            CanvasImage = image;
            CanvasControl = canvas;
        }
        
        public virtual void Activate()
        {
            IsActive = true;
            EmitSignal(SignalName.ToolActivated);
            GD.Print($"[Tool] {ToolName} activated");
        }
        
        public virtual void Deactivate()
        {
            IsActive = false;
            EmitSignal(SignalName.ToolDeactivated);
            GD.Print($"[Tool] {ToolName} deactivated");
        }
        
        /// <summary>
        /// Called when mouse/touch is pressed on canvas
        /// </summary>
        public virtual void OnPress(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
        }
        
        /// <summary>
        /// Called when mouse/touch is dragged on canvas
        /// </summary>
        public virtual void OnDrag(Vector2 fromPosition, Vector2 toPosition, Vector2 canvasFrom, Vector2 canvasTo)
        {
            if (!IsActive) return;
        }
        
        /// <summary>
        /// Called when mouse/touch is released on canvas
        /// </summary>
        public virtual void OnRelease(Vector2 position, Vector2 canvasPosition)
        {
            if (!IsActive) return;
        }
        
        /// <summary>
        /// Called every frame while tool is active
        /// </summary>
        public virtual void Process(double delta)
        {
            if (!IsActive) return;
        }
        
        /// <summary>
        /// Draw tool preview/preview overlay
        /// </summary>
        public virtual void DrawPreview(CanvasItem canvasItem, Vector2 position)
        {
            // Override in derived classes to draw previews
        }
        
        /// <summary>
        /// Locks the canvas for editing (thread-safe)
        /// </summary>
        protected void LockCanvas()
        {
            if (CanvasImage != null)
                CanvasImage.Lock();
        }
        
        /// <summary>
        /// Unlocks the canvas after editing
        /// </summary>
        protected void UnlockCanvas()
        {
            if (CanvasImage != null)
                CanvasImage.Unlock();
        }
        
        /// <summary>
        /// Updates the canvas texture after modifications
        /// </summary>
        protected void UpdateCanvas()
        {
            if (CanvasTexture != null && CanvasImage != null)
                CanvasTexture.Update(CanvasImage);
        }
        
        /// <summary>
        /// Gets color at specific position on canvas
        /// </summary>
        protected Color GetPixelColor(int x, int y)
        {
            if (CanvasImage == null) return Colors.Transparent;
            return CanvasImage.GetPixel(x, y);
        }
        
        /// <summary>
        /// Sets color at specific position on canvas
        /// </summary>
        protected void SetPixelColor(int x, int y, Color color)
        {
            if (CanvasImage == null) return;
            
            // Bounds checking
            if (x < 0 || y < 0 || x >= CanvasImage.GetWidth() || y >= CanvasImage.GetHeight())
                return;
                
            CanvasImage.SetPixel(x, y, color);
        }
        
        /// <summary>
        /// Draws a circle on the canvas (for brush strokes)
        /// </summary>
        protected void DrawCircleOnCanvas(Vector2 center, float radius, Color color)
        {
            if (CanvasImage == null) return;
            
            int centerX = (int)center.X;
            int centerY = (int)center.Y;
            int r = (int)radius;
            
            // Simple circle drawing algorithm
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        
                        if (pixelX >= 0 && pixelY >= 0 && 
                            pixelX < CanvasImage.GetWidth() && 
                            pixelY < CanvasImage.GetHeight())
                        {
                            Color existingColor = CanvasImage.GetPixel(pixelX, pixelY);
                            Color newColor = new Color(
                                existingColor.R + (color.R - existingColor.R) * color.A,
                                existingColor.G + (color.G - existingColor.G) * color.A,
                                existingColor.B + (color.B - existingColor.B) * color.A,
                                Mathf.Min(existingColor.A + color.A, 1f)
                            );
                            CanvasImage.SetPixel(pixelX, pixelY, newColor);
                        }
                    }
                }
            }
        }
        
        public override string ToString()
        {
            return $"{ToolName} - {ToolDescription}";
        }
    }
}
