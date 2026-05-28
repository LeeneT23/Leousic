using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestiona el historial de acciones para undo/redo.
/// Soporta operaciones ilimitadas (configurables) en ambas direcciones.
/// </summary>
public partial class HistoryManager : Node
{
    [Signal] public delegate void HistoryChangedEventHandler(int currentIndex, int totalActions);
    [Signal] public delegate void UndoAvailableEventHandler(bool available);
    [Signal] public delegate void RedoAvailableEventHandler(bool available);
    
    private LinkedList<HistoryAction> _undoStack = new();
    private LinkedList<HistoryAction> _redoStack = new();
    
    [Export] public int MaxHistorySize { get; set; } = 100;
    [Export] public DrawingCanvas? Canvas { get; set; }
    
    public int CurrentIndex => _undoStack.Count;
    public int TotalActions => _undoStack.Count + _redoStack.Count;
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    
    /// <summary>
    /// Agrega una nueva acción al historial
    /// </summary>
    public void AddAction(HistoryAction action)
    {
        if (action == null)
        {
            GD.PrintErr("[HistoryManager] Intento de agregar acción nula");
            return;
        }
        
        // Limpiar redo stack cuando se hace una nueva acción
        if (_redoStack.Count > 0)
        {
            ClearRedoStack();
        }
        
        // Ejecutar la acción
        action.Execute(Canvas);
        
        // Agregar al undo stack
        _undoStack.AddLast(action);
        
        // Limitar tamaño del historial
        while (_undoStack.Count > MaxHistorySize)
        {
            var oldest = _undoStack.First;
            if (oldest != null)
            {
                _undoStack.RemoveFirst();
            }
        }
        
        EmitSignals();
        GD.Print($"[HistoryManager] Acción agregada: {action.ActionName}. Undo stack: {_undoStack.Count}");
    }
    
    /// <summary>
    /// Deshace la última acción
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
        {
            GD.Print("[HistoryManager] No hay nada que deshacer");
            return;
        }
        
        var lastAction = _undoStack.Last;
        if (lastAction != null && lastAction.Value != null)
        {
            lastAction.Value.Undo(Canvas);
            _undoStack.RemoveLast();
            _redoStack.AddLast(lastAction.Value);
            
            EmitSignals();
            GD.Print($"[HistoryManager] Deshecho: {lastAction.Value.ActionName}");
        }
    }
    
    /// <summary>
    /// Rehace la última acción deshecha
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
        {
            GD.Print("[HistoryManager] No hay nada que rehacer");
            return;
        }
        
        var lastAction = _redoStack.Last;
        if (lastAction != null && lastAction.Value != null)
        {
            lastAction.Value.Execute(Canvas);
            _redoStack.RemoveLast();
            _undoStack.AddLast(lastAction.Value);
            
            EmitSignals();
            GD.Print($"[HistoryManager] Rehecho: {lastAction.Value.ActionName}");
        }
    }
    
    /// <summary>
    /// Limpia todo el historial
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        EmitSignals();
        GD.Print("[HistoryManager] Historial limpiado");
    }
    
    /// <summary>
    /// Limpia solo el stack de redo
    /// </summary>
    private void ClearRedoStack()
    {
        _redoStack.Clear();
        EmitSignals();
    }
    
    /// <summary>
    /// Emite las señales de actualización
    /// </summary>
    private void EmitSignals()
    {
        EmitSignal(SignalName.HistoryChanged, CurrentIndex, TotalActions);
        EmitSignal(SignalName.UndoAvailable, CanUndo);
        EmitSignal(SignalName.RedoAvailable, CanRedo);
    }
}

/// <summary>
/// Clase base para todas las acciones del historial
/// </summary>
public abstract class HistoryAction
{
    public string ActionName { get; protected set; } = "Acción";
    
    /// <summary>
    /// Ejecuta la acción
    /// </summary>
    public abstract void Execute(DrawingCanvas? canvas);
    
    /// <summary>
    /// Deshace la acción
    /// </summary>
    public abstract void Undo(DrawingCanvas? canvas);
}

/// <summary>
/// Acción para dibujar un trazo
/// </summary>
public class StrokeAction : HistoryAction
{
    private List<Vector2> _points;
    private Color _color;
    private float _brushSize;
    private float _opacity;
    private int _layerId;
    private Image? _previousState;
    
    public StrokeAction(List<Vector2> points, Color color, float brushSize, float opacity, int layerId)
    {
        ActionName = "Trazo";
        _points = new List<Vector2>(points);
        _color = color;
        _brushSize = brushSize;
        _opacity = opacity;
        _layerId = layerId;
    }
    
    public override void Execute(DrawingCanvas? canvas)
    {
        if (canvas == null || _points.Count == 0)
            return;
        
        // Guardar estado anterior si es la primera ejecución
        if (_previousState == null)
        {
            var layer = canvas.GetLayer(_layerId);
            if (layer != null && layer.Texture != null)
            {
                _previousState = layer.Texture.GetImage();
            }
        }
        
        // Dibujar el trazo
        var layer = canvas.GetLayer(_layerId);
        if (layer != null && layer.Texture != null)
        {
            Image img = layer.Texture.GetImage();
            img.Lock();
            
            for (int i = 1; i < _points.Count; i++)
            {
                DrawLineOnImage(img, _points[i - 1], _points[i], _color, _brushSize, _opacity);
            }
            
            img.Unlock();
            layer.Texture.Update(img);
            canvas.MarkLayerAsModified(_layerId);
        }
    }
    
    public override void Undo(DrawingCanvas? canvas)
    {
        if (canvas == null || _previousState == null)
            return;
        
        // Restaurar estado anterior
        var layer = canvas.GetLayer(_layerId);
        if (layer != null && layer.Texture != null)
        {
            Image newImage = _previousState.Duplicate();
            layer.Texture.Update(newImage);
            canvas.MarkLayerAsModified(_layerId);
        }
    }
    
    private void DrawLineOnImage(Image img, Vector2 from, Vector2 to, Color color, float brushSize, float opacity)
    {
        int radius = (int)(brushSize / 2);
        int steps = (int)from.DistanceTo(to);
        
        if (steps == 0)
        {
            DrawCircleOnImage(img, from, radius, color, opacity);
            return;
        }
        
        Vector2 direction = (to - from).Normalized();
        
        for (int i = 0; i <= steps; i++)
        {
            Vector2 pos = from + direction * i;
            DrawCircleOnImage(img, pos, radius, color, opacity);
        }
    }
    
    private void DrawCircleOnImage(Image img, Vector2 center, int radius, Color color, float opacity)
    {
        Color colorWithOpacity = new Color(color.R, color.G, color.B, color.A * opacity);
        
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = (int)(center.X + x);
                    int py = (int)(center.Y + y);
                    
                    if (px >= 0 && px < img.GetSize().X && py >= 0 && py < img.GetSize().Y)
                    {
                        Color existing = img.GetPixel(px, py);
                        Color blended = BlendColors(existing, colorWithOpacity);
                        img.SetPixel(px, py, blended);
                    }
                }
            }
        }
    }
    
    private Color BlendColors(Color background, Color foreground)
    {
        float alpha = foreground.A;
        return new Color(
            background.R * (1 - alpha) + foreground.R * alpha,
            background.G * (1 - alpha) + foreground.G * alpha,
            background.B * (1 - alpha) + foreground.B * alpha,
            Mathf.Max(background.A, foreground.A)
        );
    }
}

/// <summary>
/// Acción para mover una capa
/// </summary>
public class MoveLayerAction : HistoryAction
{
    private int _layerId;
    private Vector2 _offset;
    private Image? _previousState;
    
    public MoveLayerAction(int layerId, Vector2 offset)
    {
        ActionName = "Mover Capa";
        _layerId = layerId;
        _offset = offset;
    }
    
    public override void Execute(DrawingCanvas? canvas)
    {
        if (canvas == null)
            return;
        
        if (_previousState == null)
        {
            var layer = canvas.GetLayer(_layerId);
            if (layer != null && layer.Texture != null)
            {
                _previousState = layer.Texture.GetImage();
            }
        }
        
        var layer = canvas.GetLayer(_layerId);
        if (layer != null)
        {
            layer.Offset += _offset;
            canvas.MarkLayerAsModified(_layerId);
        }
    }
    
    public override void Undo(DrawingCanvas? canvas)
    {
        if (canvas == null)
            return;
        
        var layer = canvas.GetLayer(_layerId);
        if (layer != null)
        {
            layer.Offset -= _offset;
            canvas.MarkLayerAsModified(_layerId);
        }
    }
}
