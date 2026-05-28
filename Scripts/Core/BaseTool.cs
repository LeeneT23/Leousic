using Godot;
using System;

namespace PhotoGodot.Core;

public abstract partial class BaseTool : RefCounted
{
    public string Name { get; protected set; } = "Base Tool";
    public string Description { get; protected set; } = "Base tool description";
    public bool IsActive { get; private set; }
    
    protected DrawingCanvas? Canvas { get; private set; }
    protected LayerManager? LayerManager { get; private set; }
    protected HistoryManager? HistoryManager { get; private set; }
    
    // Tool properties
    public Color PrimaryColor { get; set; } = Colors.Black;
    public Color SecondaryColor { get; set; } = Colors.White;
    public float BrushSize { get; set; } = 10f;
    public float Opacity { get; set; } = 1.0f;
    public float Hardness { get; set; } = 1.0f;
    
    // State
    protected Vector2 LastPosition { get; private set; }
    protected bool IsDrawing { get; private set; }
    protected Layer? WorkingLayer => LayerManager?.ActiveLayer;

    public virtual void Initialize(DrawingCanvas canvas, LayerManager layerManager, HistoryManager historyManager)
    {
        Canvas = canvas;
        LayerManager = layerManager;
        HistoryManager = historyManager;
    }

    public virtual void Activate()
    {
        IsActive = true;
        OnActivate();
        GD.Print($"[Tool] Activated: {Name}");
    }

    public virtual void Deactivate()
    {
        IsActive = false;
        IsDrawing = false;
        OnDeactivate();
        GD.Print($"[Tool] Deactivated: {Name}");
    }

    public virtual void OnMouseDown(Vector2 position, MouseButton button)
    {
        if (WorkingLayer == null || !WorkingLayer.Visible) return;
        
        LastPosition = position;
        IsDrawing = true;
        
        if (button == MouseButton.Left)
        {
            OnLeftMouseDown(position);
        }
        else if (button == MouseButton.Right)
        {
            OnRightMouseDown(position);
        }
    }

    public virtual void OnMouseDrag(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!IsDrawing || WorkingLayer == null || !WorkingLayer.Visible) return;
        
        OnDraw(from, to, delta);
        LastPosition = to;
        
        Canvas?.QueueRedraw();
    }

    public virtual void OnMouseUp(Vector2 position, MouseButton button)
    {
        if (!IsDrawing) return;
        
        IsDrawing = false;
        
        if (button == MouseButton.Left)
        {
            OnLeftMouseUp(position);
        }
        else if (button == MouseButton.Right)
        {
            OnRightMouseUp(position);
        }
    }

    public virtual void OnMouseMove(Vector2 position)
    {
        // Override for cursor updates
    }

    public virtual void OnKeyDown(Keycode keycode)
    {
        // Override for keyboard shortcuts
    }

    public virtual void UpdateProperties(float size, float opacity, float hardness, Color color)
    {
        BrushSize = size;
        Opacity = opacity;
        Hardness = hardness;
        PrimaryColor = color;
    }

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }
    protected virtual void OnLeftMouseDown(Vector2 position) { }
    protected virtual void OnRightMouseDown(Vector2 position) { }
    protected virtual void OnLeftMouseUp(Vector2 position) { }
    protected virtual void OnRightMouseUp(Vector2 position) { }
    protected abstract void OnDraw(Vector2 from, Vector2 to, Vector2 delta);

    protected void SaveState(string actionName, string? description = null)
    {
        if (HistoryManager != null && WorkingLayer != null)
        {
            HistoryManager.PushAction(actionName, WorkingLayer, LayerManager!.ActiveLayerIndex, description);
        }
    }

    protected int ScreenToLayerX(float screenX)
    {
        if (Canvas == null) return (int)screenX;
        return (int)((screenX - Canvas.OffsetX) / Canvas.Zoom);
    }

    protected int ScreenToLayerY(float screenY)
    {
        if (Canvas == null) return (int)screenY;
        return (int)((screenY - Canvas.OffsetY) / Canvas.Zoom);
    }

    protected Vector2 ScreenToLayer(Vector2 screenPos)
    {
        if (Canvas == null) return screenPos;
        return new Vector2(
            (screenPos.X - Canvas.OffsetX) / Canvas.Zoom,
            (screenPos.Y - Canvas.OffsetY) / Canvas.Zoom
        );
    }
}
