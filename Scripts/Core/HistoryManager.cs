using Godot;
using System.Collections.Generic;

public partial class HistoryManager : Node
{
    private Main _main;
    private List<Image> _history = new();
    private int _currentIndex = -1;
    private int _maxHistorySize = 500;
    
    public int HistoryCount => _history.Count;
    public int CurrentIndex => _currentIndex;
    public bool CanUndo => _currentIndex > 0;
    public bool CanRedo => _currentIndex < _history.Count - 1;
    
    public void Initialize(Main main, int maxHistorySize = 500)
    {
        _main = main;
        _maxHistorySize = maxHistorySize;
    }
    
    public void SaveState(Image image)
    {
        if (image == null) return;
        
        // Remove any redo states
        while (_history.Count > _currentIndex + 1)
        {
            var oldImage = _history[_history.Count - 1];
            oldImage?.Dispose();
            _history.RemoveAt(_history.Count - 1);
        }
        
        // Save new state
        var newState = image.Duplicate() as Image;
        _history.Add(newState);
        _currentIndex++;
        
        // Limit history size
        while (_history.Count > _maxHistorySize)
        {
            var oldestImage = _history[0];
            oldestImage?.Dispose();
            _history.RemoveAt(0);
            _currentIndex--;
        }
        
        UpdateUI();
    }
    
    public void Undo()
    {
        if (!CanUndo) return;
        
        _currentIndex--;
        RestoreState();
        UpdateUI();
        GD.Print("Undo");
    }
    
    public void Redo()
    {
        if (!CanRedo) return;
        
        _currentIndex++;
        RestoreState();
        UpdateUI();
        GD.Print("Redo");
    }
    
    private void RestoreState()
    {
        if (_currentIndex < 0 || _currentIndex >= _history.Count) return;
        
        var currentState = _history[_currentIndex];
        if (currentState != null && _main.GetLayerManager() != null)
        {
            _main.GetLayerManager().RestoreFromImage(currentState);
        }
    }
    
    public void Clear()
    {
        foreach (var image in _history)
        {
            image?.Dispose();
        }
        _history.Clear();
        _currentIndex = -1;
    }
    
    private void UpdateUI()
    {
        if (_main.GetMainUI() != null)
        {
            _main.GetMainUI().UpdateStatus($"Undo: {_currentIndex + 1}/{_history.Count}");
        }
    }
}
