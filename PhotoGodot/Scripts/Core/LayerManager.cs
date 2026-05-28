using Godot;
using System.Collections.Generic;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Manages multiple layers for compositing
    /// </summary>
    public class LayerManager : Node
    {
        [Signal] public delegate void LayerAddedEventHandler(Layer layer, int index);
        [Signal] public delegate void LayerRemovedEventHandler(int index);
        [Signal] public delegate void LayerReorderedEventHandler();
        [Signal] public delegate void ActiveLayerChangedEventHandler(Layer layer);
        
        private List<Layer> _layers = new List<Layer>();
        private int _activeLayerIndex = -1;
        private Image _compositeImage;
        
        public int LayerCount => _layers.Count;
        public int ActiveLayerIndex => _activeLayerIndex;
        public Layer ActiveLayer => _activeLayerIndex >= 0 && _activeLayerIndex < _layers.Count 
            ? _layers[_activeLayerIndex] : null;
        public IReadOnlyList<Layer> Layers => _layers.AsReadOnly();
        
        public override void _Ready()
        {
            GD.Print("[LayerManager] Initialized");
        }
        
        /// <summary>
        /// Initialize with canvas size
        /// </summary>
        public void Initialize(int width, int height)
        {
            _compositeImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            _compositeImage.Fill(Colors.White);
            
            // Create background layer
            AddLayer("Background", false);
            
            GD.Print($"[LayerManager] Initialized with {width}x{height} canvas");
        }
        
        /// <summary>
        /// Add a new layer
        /// </summary>
        public Layer AddLayer(string name = "Layer", bool transparent = true, int index = -1)
        {
            var layer = new Layer();
            layer.LayerName = name;
            layer.Name = name;
            
            int width = _compositeImage?.GetWidth() ?? 1920;
            int height = _compositeImage?.GetHeight() ?? 1080;
            
            layer.Create(width, height, transparent);
            
            if (index < 0 || index >= _layers.Count)
            {
                _layers.Add(layer);
                _activeLayerIndex = _layers.Count - 1;
            }
            else
            {
                _layers.Insert(index, layer);
                if (index <= _activeLayerIndex)
                    _activeLayerIndex++;
            }
            
            AddChild(layer);
            CompositeAll();
            
            EmitSignal(SignalName.LayerAdded, layer, GetLayerIndex(layer));
            EmitSignal(SignalName.ActiveLayerChanged, ActiveLayer);
            
            GD.Print($"[LayerManager] Added layer: {name}");
            return layer;
        }
        
        /// <summary>
        /// Remove a layer
        /// </summary>
        public void RemoveLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
            {
                GD.PrintErr($"[LayerManager] Invalid layer index: {index}");
                return;
            }
            
            if (_layers.Count == 1)
            {
                GD.PrintErr("[LayerManager] Cannot remove the last layer");
                return;
            }
            
            var layer = _layers[index];
            _layers.RemoveAt(index);
            layer.QueueFree();
            
            if (index == _activeLayerIndex)
            {
                _activeLayerIndex = Mathf.Max(0, _layers.Count - 1);
            }
            else if (index < _activeLayerIndex)
            {
                _activeLayerIndex--;
            }
            
            CompositeAll();
            
            EmitSignal(SignalName.LayerRemoved, index);
            EmitSignal(SignalName.ActiveLayerChanged, ActiveLayer);
            
            GD.Print($"[LayerManager] Removed layer at index {index}");
        }
        
        /// <summary>
        /// Move layer to new position
        /// </summary>
        public void MoveLayer(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _layers.Count ||
                toIndex < 0 || toIndex >= _layers.Count)
                return;
            
            var layer = _layers[fromIndex];
            _layers.RemoveAt(fromIndex);
            _layers.Insert(toIndex, layer);
            
            // Update active layer index if needed
            if (fromIndex == _activeLayerIndex)
            {
                _activeLayerIndex = toIndex;
            }
            else if (fromIndex < _activeLayerIndex && toIndex >= _activeLayerIndex)
            {
                _activeLayerIndex--;
            }
            else if (fromIndex > _activeLayerIndex && toIndex <= _activeLayerIndex)
            {
                _activeLayerIndex++;
            }
            
            CompositeAll();
            EmitSignal(SignalName.LayerReordered);
            EmitSignal(SignalName.ActiveLayerChanged, ActiveLayer);
        }
        
        /// <summary>
        /// Set active layer
        /// </summary>
        public void SetActiveLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
                return;
                
            _activeLayerIndex = index;
            EmitSignal(SignalName.ActiveLayerChanged, ActiveLayer);
        }
        
        /// <summary>
        /// Get layer by index
        /// </summary>
        public Layer GetLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
                return null;
            return _layers[index];
        }
        
        /// <summary>
        /// Get layer index
        /// </summary>
        public int GetLayerIndex(Layer layer)
        {
            return _layers.IndexOf(layer);
        }
        
        /// <summary>
        /// Composite all visible layers
        /// </summary>
        public void CompositeAll()
        {
            if (_compositeImage == null) return;
            
            _compositeImage.Lock();
            _compositeImage.Fill(new Color(0, 0, 0, 0));
            
            foreach (var layer in _layers)
            {
                if (layer.IsVisible)
                {
                    layer.CompositeOnto(_compositeImage);
                }
            }
            
            _compositeImage.Unlock();
        }
        
        /// <summary>
        /// Get composite image
        /// </summary>
        public Image GetCompositeImage()
        {
            CompositeAll();
            return _compositeImage?.Duplicate();
        }
        
        /// <summary>
        /// Merge all visible layers into one
        /// </summary>
        public void MergeVisibleLayers()
        {
            CompositeAll();
            
            // Clear all layers except first
            while (_layers.Count > 1)
            {
                RemoveLayer(_layers.Count - 1);
            }
            
            // Set merged content to remaining layer
            if (_layers.Count > 0 && _compositeImage != null)
            {
                _layers[0].FromImage(_compositeImage.Duplicate(), "Merged");
            }
            
            GD.Print("[LayerManager] Merged visible layers");
        }
        
        /// <summary>
        /// Duplicate current layer
        /// </summary>
        public Layer DuplicateLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
                return null;
                
            var sourceLayer = _layers[index];
            var newLayer = AddLayer($"{sourceLayer.LayerName} copy", true, index + 1);
            
            if (sourceLayer.Image != null && newLayer.Image != null)
            {
                newLayer.Image.CopyDataFrom(sourceLayer.Image);
                newLayer.UpdateTexture();
            }
            
            return newLayer;
        }
        
        /// <summary>
        /// Clear all layers
        /// </summary>
        public void ClearAll()
        {
            while (_layers.Count > 0)
            {
                var layer = _layers[0];
                _layers.RemoveAt(0);
                layer.QueueFree();
            }
            
            _activeLayerIndex = -1;
            CompositeAll();
        }
    }
}
