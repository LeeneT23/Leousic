using Godot;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class HistoryManager : Node
{
    [Export] public int MaxHistorySize { get; set; } = 50;

    private Stack<HistoryState> _undoStack = new();
    private Stack<HistoryState> _redoStack = new();
    
    private LayerManager _layerManager;

    public void Setup(LayerManager layerManager)
    {
        _layerManager = layerManager;
    }

    public void SaveState(string actionName)
    {
        if (_layerManager.ActiveLayer == null) return;

        var state = new HistoryState
        {
            ActionName = actionName,
            LayerIndex = _layerManager.Layers.IndexOf(_layerManager.ActiveLayer),
            Snapshot = _layerManager.ActiveLayer.GetSnapshot()
        };

        _undoStack.Push(state);
        if (_undoStack.Count > MaxHistorySize)
            _undoStack.Pop();
        
        _redoStack.Clear();
        GD.Print($"Historial: {actionName} (Undo: {_undoStack.Count})");
    }

    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        var state = _undoStack.Pop();
        
        // Guardar estado actual para redo
        var currentLayer = _layerManager.Layers[state.LayerIndex];
        var currentState = new HistoryState
        {
            ActionName = $"Deshacer {state.ActionName}",
            LayerIndex = state.LayerIndex,
            Snapshot = currentLayer.GetSnapshot()
        };
        _redoStack.Push(currentState);

        // Restaurar
        currentLayer.RestoreSnapshot(state.Snapshot);
        GD.Print("Deshacer acción");
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        var state = _redoStack.Pop();
        
        // Guardar para undo
        var currentLayer = _layerManager.Layers[state.LayerIndex];
        var currentState = new HistoryState
        {
            ActionName = $"Rehacer {state.ActionName}",
            LayerIndex = state.LayerIndex,
            Snapshot = currentLayer.GetSnapshot()
        };
        _undoStack.Push(currentState);

        currentLayer.RestoreSnapshot(state.Snapshot);
        GD.Print("Rehacer acción");
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    private class HistoryState
    {
        public string ActionName { get; set; } = "";
        public int LayerIndex { get; set; }
        public Image Snapshot { get; set; } = null!;
    }
}
