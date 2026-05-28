using Godot;
using System.Collections.Generic;

public partial class LayerManager : Node
{
    private Main _main;
    private List<Layer> _layers = new();
    private int _activeLayerIndex = -1;
    private int _canvasWidth;
    private int _canvasHeight;
    
    public int LayerCount => _layers.Count;
    public int ActiveLayerIndex => _activeLayerIndex;
    public Layer ActiveLayer => _activeLayerIndex >= 0 && _activeLayerIndex < _layers.Count ? _layers[_activeLayerIndex] : null;
    
    public void Initialize(Main main, int canvasWidth, int canvasHeight)
    {
        _main = main;
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
    }
    
    public int CreateLayer(string name = "Layer")
    {
        var layer = new Layer(_canvasWidth, _canvasHeight, name);
        _layers.Add(layer);
        
        if (_activeLayerIndex == -1)
        {
            _activeLayerIndex = 0;
        }
        else
        {
            _activeLayerIndex = _layers.Count - 1;
        }
        
        GD.Print($"Layer created: {name} (Index: {_activeLayerIndex})");
        UpdateUI();
        return _layers.Count - 1;
    }
    
    public void CreateLayerFromImage(Image image, string name = "Imported Layer")
    {
        var layer = new Layer(image.GetWidth(), image.GetHeight(), name);
        var layerImage = layer.GetImage();
        layerImage.BlendRect(image, new Rect2i(0, 0, image.GetWidth(), image.GetHeight()), Vector2I.Zero);
        _layers.Add(layer);
        _activeLayerIndex = _layers.Count - 1;
        GD.Print($"Layer created from image: {name}");
        UpdateUI();
    }
    
    public void DeleteLayer(int index)
    {
        if (index < 0 || index >= _layers.Count) return;
        
        _layers.RemoveAt(index);
        
        if (_activeLayerIndex >= _layers.Count)
        {
            _activeLayerIndex = _layers.Count - 1;
        }
        
        GD.Print($"Layer deleted: Index {index}");
        UpdateUI();
    }
    
    public void SetActiveLayer(int index)
    {
        if (index >= 0 && index < _layers.Count)
        {
            _activeLayerIndex = index;
            GD.Print($"Active layer: {index} ({_layers[index].Name})");
            UpdateUI();
        }
    }
    
    public void MoveLayer(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _layers.Count || 
            toIndex < 0 || toIndex >= _layers.Count) return;
        
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
        
        UpdateUI();
    }
    
    public void DuplicateLayer(int index)
    {
        if (index < 0 || index >= _layers.Count) return;
        
        var sourceLayer = _layers[index];
        var newLayer = new Layer(_canvasWidth, _canvasHeight, $"{sourceLayer.Name} Copy");
        var newImage = newLayer.GetImage();
        newImage.BlendRect(sourceLayer.GetImage(), new Rect2i(0, 0, _canvasWidth, _canvasHeight), Vector2I.Zero);
        newLayer.Opacity = sourceLayer.Opacity;
        newLayer.Visible = sourceLayer.Visible;
        
        _layers.Insert(index + 1, newLayer);
        _activeLayerIndex = index + 1;
        
        GD.Print($"Layer duplicated: {newLayer.Name}");
        UpdateUI();
    }
    
    public void MergeDown(int index)
    {
        if (index <= 0 || index >= _layers.Count) return;
        
        var topLayer = _layers[index];
        var bottomLayer = _layers[index - 1];
        
        var bottomImage = bottomLayer.GetImage();
        var topImage = topLayer.GetImage();
        
        bottomImage.BlendRect(topImage, new Rect2i(0, 0, _canvasWidth, _canvasHeight), Vector2I.Zero);
        
        _layers.RemoveAt(index);
        _activeLayerIndex = index - 1;
        
        GD.Print($"Layer merged down: {topLayer.Name} -> {bottomLayer.Name}");
        UpdateUI();
    }
    
    public void ClearAllLayers()
    {
        _layers.Clear();
        _activeLayerIndex = -1;
        GD.Print("All layers cleared");
    }
    
    public Image GetCompositedImage()
    {
        if (_layers.Count == 0) return null;
        
        var result = Image.CreateEmpty(_canvasWidth, _canvasHeight, false, Image.Format.Rgba8);
        result.Fill(Colors.Transparent);
        
        foreach (var layer in _layers)
        {
            if (layer.Visible)
            {
                result.BlendRect(layer.GetImage(), new Rect2i(0, 0, _canvasWidth, _canvasHeight), Vector2I.Zero);
            }
        }
        
        return result;
    }
    
    public void RestoreFromImage(Image image)
    {
        if (image == null || _layers.Count == 0) return;
        
        var activeLayer = ActiveLayer;
        if (activeLayer != null)
        {
            var layerImage = activeLayer.GetImage();
            layerImage.BlendRect(image, new Rect2i(0, 0, image.GetWidth(), image.GetHeight()), Vector2I.Zero);
        }
    }
    
    public void ExportToPNG(string path)
    {
        var compositedImage = GetCompositedImage();
        if (compositedImage != null)
        {
            compositedImage.SavePng(path);
            GD.Print($"Exported to PNG: {path}");
        }
    }
    
    public void ApplyFilterToActiveLayer(System.Func<Color, Color> filterFunc)
    {
        if (ActiveLayer == null) return;
        
        var image = ActiveLayer.GetImage();
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixel = image.GetPixel(x, y);
                Color filtered = filterFunc(pixel);
                image.SetPixel(x, y, filtered);
            }
        }
        
        GD.Print($"Filter applied to: {ActiveLayer.Name}");
    }
    
    private void UpdateUI()
    {
        if (_main.GetMainUI() != null)
        {
            _main.GetMainUI().UpdateLayersList();
        }
    }
}
