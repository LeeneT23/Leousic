using Godot;
using System;

namespace PhotoGodot.Core;

public partial class DrawingCanvas : Control
{
    public int CanvasWidth { get; private set; } = 1024;
    public int CanvasHeight { get; private set; } = 768;
    
    public float Zoom { get; private set; } = 1.0f;
    public float MinZoom => 0.1f;
    public float MaxZoom => 10.0f;
    
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }
    
    public bool ShowGrid { get; set; } = false;
    public int GridSize { get; set; } = 50;
    
    public LayerManager? LayerManager { get; private set; }
    public ToolManager? ToolManager { get; private set; }
    
    public Color GridColorMajor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color GridColorMinor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.3f);
    public Color BackgroundColor { get; set; } = new Color(0.15f, 0.15f, 0.15f);
    
    private bool _isPanning = false;
    private Vector2 _panStart;
    private Vector2 _offsetStart;
    
    // Checkerboard pattern for transparency
    private ImageTexture? _checkerboardTexture;
    
    public event Action<float>? OnZoomChanged;
    public event Action? OnViewChanged;

    public override void _Ready()
    {
        FocusMode = Control.FocusModeEnum.All;
        
        // Create checkerboard texture
        CreateCheckerboard();
        
        MouseFilter = Control.MouseFilterEnum.Stop;
    }

    public void Initialize(LayerManager layerManager, ToolManager toolManager)
    {
        LayerManager = layerManager;
        ToolManager = toolManager;
        
        CenterCanvas();
        QueueRedraw();
    }

    public void SetCanvasSize(int width, int height)
    {
        CanvasWidth = width;
        CanvasHeight = height;
        CenterCanvas();
        QueueRedraw();
    }

    public override void _Draw()
    {
        // Draw background
        DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor);
        
        if (LayerManager == null) return;
        
        var canvasRect = GetCanvasRect();
        
        // Draw checkerboard for transparency
        DrawCheckerboard(canvasRect);
        
        // Draw all layers
        foreach (var layer in LayerManager.GetAllLayers())
        {
            if (!layer.Visible || layer.Texture == null) continue;
            
            var destRect = new Rect2(
                canvasRect.Position.X + layer.Width * 0,
                canvasRect.Position.Y + layer.Height * 0,
                layer.Width * Zoom,
                layer.Height * Zoom
            );
            
            DrawTextureRect(layer.Texture, destRect, false);
        }
        
        // Draw grid
        if (ShowGrid)
        {
            DrawGrid(canvasRect);
        }
        
        // Draw selection
        if (ToolManager?.CurrentTool is Tools.SelectTool selectTool && selectTool.HasSelection)
        {
            DrawSelection(selectTool.SelectionRect);
        }
        
        // Draw cursor preview
        DrawCursorPreview();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isPanning)
            {
                var delta = mouseMotion.Position - _panStart;
                OffsetX = _offsetStart.X + delta.X;
                OffsetY = _offsetStart.Y + delta.Y;
                QueueRedraw();
                OnViewChanged?.Invoke();
            }
            else
            {
                ToolManager?.CurrentTool?.OnMouseMove(mouseMotion.Position);
            }
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            // Middle mouse or Space+Left for panning
            if (mouseButton.ButtonIndex == MouseButton.Middle || 
                (mouseButton.ButtonIndex == MouseButton.Left && Input.IsKeyPressed(Key.Space)))
            {
                if (mouseButton.Pressed)
                {
                    _isPanning = true;
                    _panStart = mouseButton.Position;
                    _offsetStart = new Vector2(OffsetX, OffsetY);
                }
                else
                {
                    _isPanning = false;
                }
                
                ((Control)this).GrabFocus();
                return;
            }
            
            // Handle scroll for zoom
            if (mouseButton.ButtonIndex == MouseButton.WheelUp || 
                mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                var zoomDelta = mouseButton.ButtonIndex == MouseButton.WheelUp ? 0.1f : -0.1f;
                if (Input.IsKeyPressed(Key.Ctrl))
                {
                    zoomDelta *= 2;
                }
                
                SetZoom(Zoom + zoomDelta, mouseButton.Position);
                ((Control)this).GrabFocus();
                return;
            }
            
            // Pass to tool manager
            if (mouseButton.ButtonIndex == MouseButton.Left || 
                mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (mouseButton.Pressed)
                {
                    ToolManager?.CurrentTool?.OnMouseDown(mouseButton.Position, mouseButton.ButtonIndex);
                }
                else
                {
                    ToolManager?.CurrentTool?.OnMouseUp(mouseButton.Position, mouseButton.ButtonIndex);
                }
                
                ((Control)this).GrabFocus();
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Space)
                {
                    // Space for pan mode
                }
                else if (keyEvent.Keycode == Key.G)
                {
                    ToggleGrid();
                }
                else
                {
                    ToolManager?.CurrentTool?.OnKeyDown(keyEvent.Keycode);
                }
            }
        }
    }

    public Rect2 GetCanvasRect()
    {
        var scaledWidth = CanvasWidth * Zoom;
        var scaledHeight = CanvasHeight * Zoom;
        
        return new Rect2(
            OffsetX + (Size.X - scaledWidth) / 2,
            OffsetY + (Size.Y - scaledHeight) / 2,
            scaledWidth,
            scaledHeight
        );
    }

    public void SetZoom(float newZoom, Vector2? focusPoint = null)
    {
        var targetZoom = Mathf.Clamp(newZoom, MinZoom, MaxZoom);
        
        if (focusPoint.HasValue)
        {
            // Zoom towards mouse position
            var focus = focusPoint.Value;
            var before = WorldToCanvas(focus);
            
            Zoom = targetZoom;
            QueueRedraw();
            
            var after = CanvasToWorld(before);
            OffsetX += focus.X - after.X;
            OffsetY += focus.Y - after.Y;
        }
        else
        {
            Zoom = targetZoom;
        }
        
        OnZoomChanged?.Invoke(Zoom);
        QueueRedraw();
    }

    public void ZoomIn()
    {
        SetZoom(Zoom * 1.2f);
    }

    public void ZoomOut()
    {
        SetZoom(Zoom / 1.2f);
    }

    public void FitToWindow()
    {
        if (CanvasWidth <= 0 || CanvasHeight <= 0) return;
        
        var scaleX = Size.X / CanvasWidth;
        var scaleY = Size.Y / CanvasHeight;
        var newZoom = Math.Min(scaleX, scaleY) * 0.95f;
        
        SetZoom(newZoom);
        CenterCanvas();
    }

    public void CenterCanvas()
    {
        OffsetX = 0;
        OffsetY = 0;
        QueueRedraw();
    }

    public void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
        QueueRedraw();
    }

    public Vector2 CanvasToWorld(Vector2 canvasPos)
    {
        var canvasRect = GetCanvasRect();
        return new Vector2(
            canvasRect.Position.X + canvasPos.X * Zoom,
            canvasRect.Position.Y + canvasPos.Y * Zoom
        );
    }

    public Vector2 WorldToCanvas(Vector2 worldPos)
    {
        var canvasRect = GetCanvasRect();
        return new Vector2(
            (worldPos.X - canvasRect.Position.X) / Zoom,
            (worldPos.Y - canvasRect.Position.Y) / Zoom
        );
    }

    private void CreateCheckerboard()
    {
        var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                bool isEven = (x / 8 + y / 8) % 2 == 0;
                img.SetPixel(x, y, isEven ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.6f, 0.6f, 0.6f));
            }
        }
        
        _checkerboardTexture = ImageTexture.CreateFromImage(img);
    }

    private void DrawCheckerboard(Rect2 canvasRect)
    {
        if (_checkerboardTexture == null) return;
        
        var tileCountX = (int)Mathf.Ceil(canvasRect.Size.X / 32);
        var tileCountY = (int)Mathf.Ceil(canvasRect.Size.Y / 32);
        
        for (int y = 0; y < tileCountY; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {
                var destRect = new Rect2(
                    canvasRect.Position.X + x * 32,
                    canvasRect.Position.Y + y * 32,
                    32,
                    32
                );
                DrawTextureRect(_checkerboardTexture, destRect, false);
            }
        }
    }

    private void DrawGrid(Rect2 canvasRect)
    {
        var gridSize = GridSize * Zoom;
        
        // Vertical lines
        for (float x = canvasRect.Position.X; x < canvasRect.End.X; x += gridSize)
        {
            var isMajor = ((x - canvasRect.Position.X) / gridSize) % 5 == 0;
            var color = isMajor ? GridColorMajor : GridColorMinor;
            DrawLine(new Vector2(x, canvasRect.Position.Y), new Vector2(x, canvasRect.End.Y), color, 1);
        }
        
        // Horizontal lines
        for (float y = canvasRect.Position.Y; y < canvasRect.End.Y; y += gridSize)
        {
            var isMajor = ((y - canvasRect.Position.Y) / gridSize) % 5 == 0;
            var color = isMajor ? GridColorMajor : GridColorMinor;
            DrawLine(new Vector2(canvasRect.Position.X, y), new Vector2(canvasRect.End.X, y), color, 1);
        }
    }

    private void DrawSelection(Rect2 selectionRect)
    {
        var worldRect = new Rect2(
            CanvasToWorld(selectionRect.Position),
            selectionRect.Size * Zoom
        );
        
        // Draw marching ants border
        var borderColor = Colors.White;
        DrawRect(worldRect, borderColor, false, 2);
        
        // Draw semi-transparent overlay outside selection
        var canvasRect = GetCanvasRect();
        
        // Top
        DrawRect(new Rect2(canvasRect.Position.X, canvasRect.Position.Y, canvasRect.Size.X, worldRect.Position.Y - canvasRect.Position.Y), 
            new Color(0, 0, 0, 0.3f));
        // Bottom
        DrawRect(new Rect2(canvasRect.Position.X, worldRect.End.Y, canvasRect.Size.X, canvasRect.End.Y - worldRect.End.Y), 
            new Color(0, 0, 0, 0.3f));
        // Left
        DrawRect(new Rect2(canvasRect.Position.X, worldRect.Position.Y, worldRect.Position.X - canvasRect.Position.X, worldRect.Size.Y), 
            new Color(0, 0, 0, 0.3f));
        // Right
        DrawRect(new Rect2(worldRect.End.X, worldRect.Position.Y, canvasRect.End.X - worldRect.End.X, worldRect.Size.Y), 
            new Color(0, 0, 0, 0.3f));
    }

    private void DrawCursorPreview()
    {
        if (ToolManager?.CurrentTool == null) return;
        
        var tool = ToolManager.CurrentTool;
        var mousePos = GetGlobalMousePosition();
        var canvasPos = WorldToCanvas(mousePos);
        
        // Only draw brush preview for brush-like tools
        if (tool is Tools.BrushTool or Tools.EraserTool)
        {
            var radius = tool.BrushSize * Zoom / 2;
            
            // Draw circle outline
            DrawCircle(mousePos, radius, new Color(1, 1, 1, 0.5f), false, 1);
            DrawCircle(mousePos, radius, new Color(0, 0, 0, 0.5f), false, 1);
            
            // Draw center point
            DrawRect(new Rect2(mousePos.X - 1, mousePos.Y - 1, 2, 2), Colors.Red);
        }
    }
}
