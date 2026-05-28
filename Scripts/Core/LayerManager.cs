using Godot;
using System;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class LayerManager : Node
{
    private readonly List<Layer> _layers = new();
    private int _activeLayerIndex = -1;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int LayerCount => _layers.Count;
    public int ActiveLayerIndex 
    { 
        get => _activeLayerIndex;
        set
        {
            if (value >= 0 && value < _layers.Count)
            {
                _activeLayerIndex = value;
                OnActiveLayerChanged?.Invoke(_activeLayerIndex);
            }
        }
    }
    
    public Layer? ActiveLayer => _activeLayerIndex >= 0 && _activeLayerIndex < _layers.Count 
        ? _layers[_activeLayerIndex] 
        : null;

    public event Action<int>? OnActiveLayerChanged;
    public event Action? OnLayersChanged;

    public void Initialize(int width, int height)
    {
        Width = width;
        Height = height;
        _layers.Clear();
        _activeLayerIndex = -1;
        
        // Create initial background layer
        AddLayer("Background");
    }

    public Layer AddLayer(string name = "Layer")
    {
        var layer = new Layer(Width, Height, name);
        _layers.Add(layer);
        _activeLayerIndex = _layers.Count - 1;
        
        GD.Print($"[LayerManager] Added layer: {name} (Index: {_activeLayerIndex})");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
        
        return layer;
    }

    public Layer DuplicateLayer(int index)
    {
        if (index < 0 || index >= _layers.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        var source = _layers[index];
        var duplicate = new Layer(source.Image.Duplicate() as Image, $"{source.Name} copy");
        duplicate.Visible = source.Visible;
        duplicate.Opacity = source.Opacity;
        duplicate.BlendMode = source.BlendMode;
        
        _layers.Insert(index + 1, duplicate);
        _activeLayerIndex = index + 1;
        
        GD.Print($"[LayerManager] Duplicated layer: {duplicate.Name}");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
        
        return duplicate;
    }

    public void DeleteLayer(int index)
    {
        if (index < 0 || index >= _layers.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        if (_layers.Count <= 1)
        {
            GD.PrintErr("[LayerManager] Cannot delete the last layer");
            return;
        }
        
        var deletedName = _layers[index].Name;
        _layers.RemoveAt(index);
        
        if (_activeLayerIndex >= _layers.Count)
        {
            _activeLayerIndex = _layers.Count - 1;
        }
        
        GD.Print($"[LayerManager] Deleted layer: {deletedName}");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
    }

    public void MoveLayer(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _layers.Count || 
            toIndex < 0 || toIndex >= _layers.Count)
            return;
        
        var layer = _layers[fromIndex];
        _layers.RemoveAt(fromIndex);
        _layers.Insert(toIndex, layer);
        
        if (_activeLayerIndex == fromIndex)
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
        
        GD.Print($"[LayerManager] Moved layer from {fromIndex} to {toIndex}");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
    }

    public void MergeDown(int index)
    {
        if (index <= 0 || index >= _layers.Count)
        {
            GD.PrintErr("[LayerManager] Cannot merge down: invalid index or bottom layer");
            return;
        }
        
        var top = _layers[index];
        var bottom = _layers[index - 1];
        
        // Composite top onto bottom
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var topPixel = top.Image.GetPixel(x, y);
                var bottomPixel = bottom.Image.GetPixel(x, y);
                
                // Simple alpha blend
                float a = topPixel.A;
                Color blended = new Color(
                    bottomPixel.R * (1 - a) + topPixel.R * a,
                    bottomPixel.G * (1 - a) + topPixel.G * a,
                    bottomPixel.B * (1 - a) + topPixel.B * a,
                    Mathf.Max(bottomPixel.A, topPixel.A)
                );
                
                bottom.Image.SetPixel(x, y, blended);
            }
        }
        
        bottom.UpdateTexture();
        _layers.RemoveAt(index);
        
        if (_activeLayerIndex >= index)
        {
            _activeLayerIndex = Math.Max(0, _activeLayerIndex - 1);
        }
        
        GD.Print($"[LayerManager] Merged layer {index} down");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
    }

    public void FlattenImage()
    {
        if (_layers.Count <= 1) return;
        
        var result = new Layer(Width, Height, "Flattened");
        
        // Composite all layers from bottom to top
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];
            if (!layer.Visible) continue;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var current = result.Image.GetPixel(x, y);
                    var above = layer.Image.GetPixel(x, y);
                    
                    float a = above.A;
                    Color blended = new Color(
                        current.R * (1 - a) + above.R * a,
                        current.G * (1 - a) + above.G * a,
                        current.B * (1 - a) + above.B * a,
                        Mathf.Max(current.A, above.A)
                    );
                    
                    result.Image.SetPixel(x, y, blended);
                }
            }
        }
        
        _layers.Clear();
        _layers.Add(result);
        _activeLayerIndex = 0;
        
        GD.Print("[LayerManager] Image flattened");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
    }

    public Image GetCompositedImage()
    {
        var result = Image.CreateEmpty(Width, Height, false, Image.Format.Rgba8);
        result.Fill(Colors.Transparent);
        
        // Composite from bottom to top
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];
            if (!layer.Visible) continue;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var current = result.GetPixel(x, y);
                    var above = layer.Image.GetPixel(x, y);
                    
                    float a = above.A * layer.Opacity;
                    Color blended = new Color(
                        current.R * (1 - a) + above.R * a,
                        current.G * (1 - a) + above.G * a,
                        current.B * (1 - a) + above.B * a,
                        Mathf.Max(current.A, above.A * layer.Opacity)
                    );
                    
                    result.SetPixel(x, y, blended);
                }
            }
        }
        
        return result;
    }

    public Layer? GetLayer(int index)
    {
        if (index >= 0 && index < _layers.Count)
            return _layers[index];
        return null;
    }

    public IReadOnlyList<Layer> GetAllLayers() => _layers.AsReadOnly();

    public void SetLayerVisibility(int index, bool visible)
    {
        if (index >= 0 && index < _layers.Count)
        {
            _layers[index].Visible = visible;
            OnLayersChanged?.Invoke();
        }
    }

    public void SetLayerOpacity(int index, float opacity)
    {
        if (index >= 0 && index < _layers.Count)
        {
            _layers[index].Opacity = Mathf.Clamp(opacity, 0, 1);
            _layers[index].UpdateTexture();
            OnLayersChanged?.Invoke();
        }
    }

    public void SetLayerBlendMode(int index, Layer.CanvasBlendMode mode)
    {
        if (index >= 0 && index < _layers.Count)
        {
            _layers[index].BlendMode = mode;
            OnLayersChanged?.Invoke();
        }
    }

    public void RenameLayer(int index, string newName)
    {
        if (index >= 0 && index < _layers.Count)
        {
            _layers[index].Name = newName;
            OnLayersChanged?.Invoke();
        }
    }

    public byte[] SaveProject()
    {
        var data = new Dictionary<string, object>
        {
            ["width"] = Width,
            ["height"] = Height,
            ["activeLayer"] = _activeLayerIndex,
            ["layers"] = new List<byte[]>()
        };
        
        var layerData = data["layers"] as List<byte[]>;
        foreach (var layer in _layers)
        {
            layerData.Add(layer.SaveToBytes());
        }
        
        return VarToBytes(data);
    }

    public void LoadProject(byte[] data)
    {
        var dict = BytesToVar(data) as Dictionary;
        if (dict == null) return;
        
        Width = (int)dict["width"];
        Height = (int)dict["height"];
        _activeLayerIndex = (int)dict["activeLayer"];
        
        _layers.Clear();
        var layerDataList = dict["layers"] as Godot.Collections.Array;
        
        if (layerDataList != null)
        {
            foreach (byte[] layerData in layerDataList)
            {
                var img = Image.New();
                img.LoadPngFromBuffer(layerData);
                _layers.Add(new Layer(img, "Layer"));
            }
        }
        
        GD.Print($"[LayerManager] Project loaded: {Width}x{Height}, {_layers.Count} layers");
        OnLayersChanged?.Invoke();
        OnActiveLayerChanged?.Invoke(_activeLayerIndex);
    }

    private byte[] VarToBytes(Variant variant)
    {
        var Marshaller = new Godot.Marshalls();
        return Marshaller.VarToBytes(variant);
    }

    private Variant BytesToVar(byte[] bytes)
    {
        var Marshaller = new Godot.Marshalls();
        return Marshaller.BytesToVar(bytes);
    }
}
