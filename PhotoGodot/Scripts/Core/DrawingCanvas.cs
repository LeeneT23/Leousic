using Godot;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Main canvas component for drawing and image manipulation
    /// </summary>
    public class DrawingCanvas : Control
    {
        [Signal] public delegate void CanvasClickedEventHandler(Vector2 position, Vector2 canvasPosition);
        [Signal] public delegate void CanvasResizedEventHandler(Vector2 newSize);
        
        // Canvas properties
        private Image _canvasImage;
        private ImageTexture _canvasTexture;
        
        // Viewport/zoom properties
        private Vector2 _canvasOffset = Vector2.Zero;
        private float _zoom = 1.0f;
        private float _minZoom = 0.1f;
        private float _maxZoom = 10.0f;
        
        // References to managers
        private ToolManager _toolManager;
        private HistoryManager _historyManager;
        
        // Grid visibility
        public bool ShowGrid { get; set; } = false;
        public Color GridColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 0.3f);
        public int GridSize { get; set; } = 50;
        
        // Canvas size
        public int CanvasWidth { get; private set; } = 1920;
        public int CanvasHeight { get; private set; } = 1080;
        
        public Image CanvasImage => _canvasImage;
        public ImageTexture CanvasTexture => _canvasTexture;
        public float Zoom => _zoom;
        public Vector2 Offset => _canvasOffset;
        
        public override void _Ready()
        {
            GD.Print("[DrawingCanvas] Initialized");
            
            // Find managers in the scene tree
            _toolManager = GetNodeOrNull<ToolManager>("/root/Main/ToolManager");
            _historyManager = GetNodeOrNull<HistoryManager>("/root/Main/HistoryManager");
            
            InitializeCanvas(CanvasWidth, CanvasHeight);
        }
        
        /// <summary>
        /// Initialize the canvas with specified dimensions
        /// </summary>
        public void InitializeCanvas(int width, int height)
        {
            CanvasWidth = width;
            CanvasHeight = height;
            
            // Create canvas image (RGBA8 format)
            _canvasImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            _canvasImage.Fill(Colors.White);
            
            // Create texture from image
            _canvasTexture = ImageTexture.CreateFromImage(_canvasImage);
            
            // Center canvas in viewport
            CenterCanvas();
            
            // Initialize history
            if (_historyManager != null)
            {
                _historyManager.Initialize(_canvasImage);
            }
            
            EmitSignal(SignalName.CanvasResized, new Vector2(width, height));
            GD.Print($"[DrawingCanvas] Canvas initialized: {width}x{height}");
        }
        
        /// <summary>
        /// Center the canvas in the viewport
        /// </summary>
        private void CenterCanvas()
        {
            Vector2 viewportSize = GetViewportRect().Size;
            _canvasOffset = (viewportSize - new Vector2(CanvasWidth, CanvasHeight) * _zoom) / 2f;
        }
        
        public override void _Draw()
        {
            // Draw canvas background (checkerboard for transparency)
            DrawCheckerboardBackground();
            
            // Draw canvas texture
            Rect2 canvasRect = new Rect2(_canvasOffset, new Vector2(CanvasWidth, CanvasHeight) * _zoom);
            DrawTextureRect(_canvasTexture, canvasRect, false);
            
            // Draw grid if enabled
            if (ShowGrid)
            {
                DrawGrid(canvasRect);
            }
            
            // Draw tool preview
            if (_toolManager != null)
            {
                Vector2 mousePos = GetGlobalMousePosition();
                _toolManager.DrawToolPreview(this, mousePos);
            }
            
            // Draw canvas border
            DrawRect(canvasRect, false, Colors.Black, 2f);
        }
        
        /// <summary>
        /// Draw checkerboard background pattern for transparency
        /// </summary>
        private void DrawCheckerboardBackground()
        {
            Rect2 viewportRect = GetViewportRect();
            int checkSize = 20;
            
            for (int y = 0; y < viewportRect.Size.Y; y += checkSize)
            {
                for (int x = 0; x < viewportRect.Size.X; x += checkSize)
                {
                    bool isLight = ((x / checkSize) + (y / checkSize)) % 2 == 0;
                    Color checkColor = isLight ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.7f, 0.7f, 0.7f);
                    
                    Rect2 checkRect = new Rect2(x, y, checkSize, checkSize);
                    DrawRect(checkRect, true, checkColor);
                }
            }
        }
        
        /// <summary>
        /// Draw grid overlay
        /// </summary>
        private void DrawGrid(Rect2 canvasRect)
        {
            float scaledGridSize = GridSize * _zoom;
            
            // Vertical lines
            for (float x = canvasRect.Position.X; x < canvasRect.End.X; x += scaledGridSize)
            {
                DrawLine(new Vector2(x, canvasRect.Position.Y), 
                        new Vector2(x, canvasRect.End.Y), 
                        GridColor, 1f);
            }
            
            // Horizontal lines
            for (float y = canvasRect.Position.Y; y < canvasRect.End.Y; y += scaledGridSize)
            {
                DrawLine(new Vector2(canvasRect.Position.X, y), 
                        new Vector2(canvasRect.End.X, y), 
                        GridColor, 1f);
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left || 
                    mouseButton.ButtonIndex == MouseButton.Right)
                {
                    HandleMouseInput(@event);
                }
                
                // Zoom with mouse wheel
                if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
                {
                    ZoomIn(mouseButton.Position);
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
                {
                    ZoomOut(mouseButton.Position);
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                HandleMouseInput(@event);
            }
        }
        
        /// <summary>
        /// Handle mouse input and forward to tool manager
        /// </summary>
        private void HandleMouseInput(InputEvent inputEvent)
        {
            Vector2 screenPos = GetGlobalMousePosition();
            Vector2 canvasPos = ScreenToCanvas(screenPos);
            
            // Check if mouse is over canvas
            Rect2 canvasRect = new Rect2(_canvasOffset, new Vector2(CanvasWidth, CanvasHeight) * _zoom);
            if (canvasRect.HasPoint(screenPos))
            {
                if (_toolManager != null)
                {
                    _toolManager.HandleInput(screenPos, canvasPos, inputEvent);
                }
                
                EmitSignal(SignalName.CanvasClicked, screenPos, canvasPos);
            }
        }
        
        public override void _Process(double delta)
        {
            if (_toolManager != null)
            {
                _toolManager.ProcessTools(delta);
            }
            
            QueueRedraw();
        }
        
        /// <summary>
        /// Convert screen coordinates to canvas coordinates
        /// </summary>
        public Vector2 ScreenToCanvas(Vector2 screenPos)
        {
            return (screenPos - _canvasOffset) / _zoom;
        }
        
        /// <summary>
        /// Convert canvas coordinates to screen coordinates
        /// </summary>
        public Vector2 CanvasToScreen(Vector2 canvasPos)
        {
            return canvasPos * _zoom + _canvasOffset;
        }
        
        /// <summary>
        /// Zoom in at specific position
        /// </summary>
        public void ZoomIn(Vector2 focusPoint)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Min(_zoom * 1.2f, _maxZoom);
            
            // Adjust offset to zoom towards focus point
            if (oldZoom != _zoom)
            {
                float scale = _zoom / oldZoom;
                _canvasOffset = focusPoint - (focusPoint - _canvasOffset) * scale;
            }
            
            QueueRedraw();
            GD.Print($"[DrawingCanvas] Zoom: {_zoom:F2}");
        }
        
        /// <summary>
        /// Zoom out at specific position
        /// </summary>
        public void ZoomOut(Vector2 focusPoint)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Max(_zoom / 1.2f, _minZoom);
            
            // Adjust offset to zoom towards focus point
            if (oldZoom != _zoom)
            {
                float scale = _zoom / oldZoom;
                _canvasOffset = focusPoint - (focusPoint - _canvasOffset) * scale;
            }
            
            QueueRedraw();
            GD.Print($"[DrawingCanvas] Zoom: {_zoom:F2}");
        }
        
        /// <summary>
        /// Set zoom level
        /// </summary>
        public void SetZoom(float zoom)
        {
            _zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);
            CenterCanvas();
            QueueRedraw();
        }
        
        /// <summary>
        /// Reset zoom and pan
        /// </summary>
        public void ResetView()
        {
            _zoom = 1.0f;
            CenterCanvas();
            QueueRedraw();
            GD.Print("[DrawingCanvas] View reset");
        }
        
        /// <summary>
        /// Clear canvas with specified color
        /// </summary>
        public void ClearCanvas(Color fillColor)
        {
            if (_canvasImage == null) return;
            
            _canvasImage.Lock();
            _canvasImage.Fill(fillColor);
            _canvasImage.Unlock();
            
            _canvasTexture.Update(_canvasImage);
            
            if (_historyManager != null)
            {
                _historyManager.SaveState(_canvasImage);
            }
            
            QueueRedraw();
            GD.Print("[DrawingCanvas] Canvas cleared");
        }
        
        /// <summary>
        /// Resize canvas
        /// </summary>
        public void ResizeCanvas(int newWidth, int newHeight, bool preserveContent = true)
        {
            if (!preserveContent)
            {
                InitializeCanvas(newWidth, newHeight);
                return;
            }
            
            // Create new image
            Image newImage = Image.CreateEmpty(newWidth, newHeight, false, Image.Format.Rgba8);
            newImage.Fill(Colors.Transparent);
            
            // Copy old content
            int copyWidth = Mathf.Min(CanvasWidth, newWidth);
            int copyHeight = Mathf.Min(CanvasHeight, newHeight);
            
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    Color pixel = _canvasImage.GetPixel(x, y);
                    newImage.SetPixel(x, y, pixel);
                }
            }
            
            // Replace old image
            _canvasImage = newImage;
            _canvasTexture.Update(_canvasImage);
            
            CanvasWidth = newWidth;
            CanvasHeight = newHeight;
            
            if (_historyManager != null)
            {
                _historyManager.SaveState(_canvasImage);
            }
            
            CenterCanvas();
            QueueRedraw();
            
            GD.Print($"[DrawingCanvas] Resized to: {newWidth}x{newHeight}");
        }
        
        /// <summary>
        /// Export canvas as image
        /// </summary>
        public Image ExportCanvas()
        {
            return _canvasImage.Duplicate();
        }
        
        /// <summary>
        /// Save canvas to file
        /// </summary>
        public Error SaveToFile(string path)
        {
            if (_canvasImage == null) return Error.InvalidData;
            
            Error error = _canvasImage.SavePng(path);
            
            if (error == Error.Ok)
            {
                GD.Print($"[DrawingCanvas] Saved to: {path}");
            }
            else
            {
                GD.PrintErr($"[DrawingCanvas] Failed to save: {error}");
            }
            
            return error;
        }
    }
}
