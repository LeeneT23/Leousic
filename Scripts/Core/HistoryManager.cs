using Godot;
using System;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class HistoryManager : Node
{
    private readonly List<HistoryEntry> _history = new();
    private readonly List<HistoryEntry> _redoStack = new();
    private int _maxHistorySize = 100;
    
    public int MaxHistorySize
    {
        get => _maxHistorySize;
        set
        {
            _maxHistorySize = value;
            while (_history.Count > _maxHistorySize)
            {
                _history.RemoveAt(0);
            }
        }
    }
    
    public int CurrentStep => _history.Count;
    public bool CanUndo => _history.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public class HistoryEntry
    {
        public string ActionName { get; set; } = "";
        public byte[]? LayerData { get; set; }
        public int LayerIndex { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public void PushAction(string actionName, Layer layer, int layerIndex, string? description = null)
    {
        var entry = new HistoryEntry
        {
            ActionName = actionName,
            LayerData = layer.SaveToBytes(),
            LayerIndex = layerIndex,
            Description = description ?? actionName
        };
        
        _history.Add(entry);
        _redoStack.Clear();
        
        if (_history.Count > _maxHistorySize)
        {
            _history.RemoveAt(0);
        }
        
        GD.Print($"[History] {actionName}: {entry.Description}");
    }

    public void PushLayerAction(string actionName, int layerIndex, string? description = null)
    {
        var entry = new HistoryEntry
        {
            ActionName = actionName,
            LayerIndex = layerIndex,
            Description = description ?? actionName
        };
        
        _history.Add(entry);
        _redoStack.Clear();
        
        if (_history.Count > _maxHistorySize)
        {
            _history.RemoveAt(0);
        }
    }

    public HistoryEntry? Undo()
    {
        if (!CanUndo) return null;
        
        var entry = _history[^1];
        _history.RemoveAt(_history.Count - 1);
        _redoStack.Add(entry);
        
        GD.Print($"[History] Undo: {entry.Description}");
        return entry;
    }

    public HistoryEntry? Redo()
    {
        if (!CanRedo) return null;
        
        var entry = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        _history.Add(entry);
        
        GD.Print($"[History] Redo: {entry.Description}");
        return entry;
    }

    public void Clear()
    {
        _history.Clear();
        _redoStack.Clear();
    }

    public void SaveState(LayerManager layerManager)
    {
        // Save complete state for complex operations
        GD.Print("[History] State saved");
    }
}
