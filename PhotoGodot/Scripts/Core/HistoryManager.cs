using Godot;
using System.Collections.Generic;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Manages undo/redo functionality for canvas operations
    /// </summary>
    public class HistoryManager : Node
    {
        [Signal] public delegate void HistoryChangedEventHandler(int currentIndex, int totalStates);
        [Signal] public delegate void UndoPerformedEventHandler();
        [Signal] public delegate void RedoPerformedEventHandler();
        
        private List<Image> _history = new List<Image>();
        private int _currentIndex = -1;
        private int _maxHistorySize = 50;
        
        public int CurrentIndex => _currentIndex;
        public int TotalStates => _history.Count;
        public bool CanUndo => _currentIndex > 0;
        public bool CanRedo => _currentIndex < _history.Count - 1;
        
        public override void _Ready()
        {
            GD.Print("[HistoryManager] Initialized");
        }
        
        /// <summary>
        /// Save current canvas state to history
        /// </summary>
        public void SaveState(Image canvasImage)
        {
            if (canvasImage == null) return;
            
            // Remove any redo states
            while (_history.Count > _currentIndex + 1)
            {
                var removedState = _history[_history.Count - 1];
                _history.RemoveAt(_history.Count - 1);
                removedState?.Dispose();
            }
            
            // Create a copy of the current state
            Image newState = canvasImage.Duplicate();
            _history.Add(newState);
            _currentIndex = _history.Count - 1;
            
            // Enforce max history size
            while (_history.Count > _maxHistorySize)
            {
                var oldestState = _history[0];
                _history.RemoveAt(0);
                _currentIndex--;
                oldestState?.Dispose();
            }
            
            EmitSignal(SignalName.HistoryChanged, _currentIndex, _history.Count);
            GD.Print($"[HistoryManager] State saved. Total: {_history.Count}");
        }
        
        /// <summary>
        /// Undo last action
        /// </summary>
        public Image Undo()
        {
            if (!CanUndo)
            {
                GD.Print("[HistoryManager] Cannot undo - at beginning");
                return null;
            }
            
            _currentIndex--;
            Image previousState = _history[_currentIndex].Duplicate();
            
            EmitSignal(SignalName.HistoryChanged, _currentIndex, _history.Count);
            EmitSignal(SignalName.UndoPerformed);
            GD.Print("[HistoryManager] Undo performed");
            
            return previousState;
        }
        
        /// <summary>
        /// Redo last undone action
        /// </summary>
        public Image Redo()
        {
            if (!CanRedo)
            {
                GD.Print("[HistoryManager] Cannot redo - at end");
                return null;
            }
            
            _currentIndex++;
            Image nextState = _history[_currentIndex].Duplicate();
            
            EmitSignal(SignalName.HistoryChanged, _currentIndex, _history.Count);
            EmitSignal(SignalName.RedoPerformed);
            GD.Print("[HistoryManager] Redo performed");
            
            return nextState;
        }
        
        /// <summary>
        /// Clear all history
        /// </summary>
        public void Clear()
        {
            foreach (var state in _history)
            {
                state?.Dispose();
            }
            _history.Clear();
            _currentIndex = -1;
            
            EmitSignal(SignalName.HistoryChanged, _currentIndex, 0);
            GD.Print("[HistoryManager] History cleared");
        }
        
        /// <summary>
        /// Set maximum history size
        /// </summary>
        public void SetMaxHistorySize(int size)
        {
            _maxHistorySize = Mathf.Max(1, size);
            
            // Trim history if needed
            while (_history.Count > _maxHistorySize)
            {
                var oldestState = _history[0];
                _history.RemoveAt(0);
                _currentIndex--;
                oldestState?.Dispose();
            }
            
            GD.Print($"[HistoryManager] Max history size set to: {_maxHistorySize}");
        }
        
        /// <summary>
        /// Initialize with first state
        /// </summary>
        public void Initialize(Image canvasImage)
        {
            Clear();
            SaveState(canvasImage);
        }
        
        /// <summary>
        /// Get preview of state at specific index
        /// </summary>
        public Image GetStateAt(int index)
        {
            if (index < 0 || index >= _history.Count)
                return null;
                
            return _history[index].Duplicate();
        }
        
        /// <summary>
        /// Jump to specific history state
        /// </summary>
        public Image JumpToState(int index)
        {
            if (index < 0 || index >= _history.Count)
            {
                GD.PrintErr($"[HistoryManager] Invalid state index: {index}");
                return null;
            }
            
            _currentIndex = index;
            Image state = _history[_currentIndex].Duplicate();
            
            EmitSignal(SignalName.HistoryChanged, _currentIndex, _history.Count);
            return state;
        }
    }
}
