using Godot;
using System.Collections.Generic;
using System.Linq;

namespace PhotoGodot.Core;

public partial class LayerManager : Node2D
{
    public signal LayerListChanged();
    public signal ActiveLayerChanged(Layer layer);

    private List<Layer> _layers = new();
    private Layer _activeLayer;

    public int LayerCount => _layers.Count;
    public Layer ActiveLayer => _activeLayer;
    public IReadOnlyList<Layer> Layers => _layers.AsReadOnly();

    public void Setup(int width, int height, Color bg)
    {
        // Limpiar capas existentes
        foreach (var layer in _layers)
        {
            layer.Dispose();
        }
        _layers.Clear();
        
        CreateLayer("Fondo", bg);
    }

    public Layer CreateLayer(string name, Color? fillColor = null)
    {
        var layer = new Layer();
        layer.LayerName = name;
        
        int w = _layers.Count > 0 ? _layers[0].Width : 1920;
        int h = _layers.Count > 0 ? _layers[0].Height : 1080;
        
        layer.Initialize(w, h, fillColor ?? Colors.Transparent);
        
        _layers.Insert(0, layer); // Nueva capa arriba
        if (_activeLayer == null) _activeLayer = layer;

        LayerListChanged.Emit();
        ActiveLayerChanged.Emit(_activeLayer);
        return layer;
    }

    public void RemoveLayer(Layer layer)
    {
        if (_layers.Count <= 1) return; // No borrar la última
        
        int idx = _layers.IndexOf(layer);
        _layers.Remove(layer);
        
        if (_activeLayer == layer)
        {
            _activeLayer = _layers[System.Math.Max(0, idx - 1)];
            ActiveLayerChanged.Emit(_activeLayer);
        }
        
        LayerListChanged.Emit();
    }

    public void SetActiveLayer(Layer layer)
    {
        if (_layers.Contains(layer))
        {
            _activeLayer = layer;
            ActiveLayerChanged.Emit(layer);
        }
    }

    public void MoveLayerUp(int index)
    {
        if (index <= 0 || index >= _layers.Count) return;
        var layer = _layers[index];
        _layers.RemoveAt(index);
        _layers.Insert(index - 1, layer);
        LayerListChanged.Emit();
    }

    public void MoveLayerDown(int index)
    {
        if (index < 0 || index >= _layers.Count - 1) return;
        var layer = _layers[index];
        _layers.RemoveAt(index);
        _layers.Insert(index + 1, layer);
        LayerListChanged.Emit();
    }

    public void DuplicateLayer(Layer layer)
    {
        int idx = _layers.IndexOf(layer);
        if (idx == -1) return;
        
        var newLayer = new Layer();
        newLayer.LayerName = $"{layer.LayerName} copia";
        newLayer.Initialize(layer.Width, layer.Height, Colors.Transparent);
        
        // Copiar píxeles
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                newLayer.ImageData.SetPixel(x, y, layer.GetPixel(new Vector2(x, y)));
            }
        }
        newLayer.UpdateTexture();
        
        _layers.Insert(idx + 1, newLayer);
        _activeLayer = newLayer;
        LayerListChanged.Emit();
        ActiveLayerChanged.Emit(_activeLayer);
    }

    public void MergeDown()
    {
        int idx = _layers.IndexOf(_activeLayer);
        if (idx <= 0) return;

        var top = _activeLayer;
        var bottom = _layers[idx - 1];

        var imgTop = top.ImageData;
        var imgBottom = bottom.ImageData;

        for (int y = 0; y < imgTop.GetHeight(); y++)
        {
            for (int x = 0; x < imgTop.GetWidth(); x++)
            {
                var cTop = imgTop.GetPixel(x, y);
                if (cTop.A > 0.001f)
                {
                    var cBot = imgBottom.GetPixel(x, y);
                    float a = cTop.A + cBot.A * (1 - cTop.A);
                    if (a > 0)
                    {
                        Vector3 rgb = (cTop.Rgb * cTop.A + cBot.Rgb * cBot.A * (1 - cTop.A)) / a;
                        imgBottom.SetPixel(x, y, new Color(rgb.X, rgb.Y, rgb.Z, a));
                    }
                }
            }
        }
        
        bottom.UpdateTexture();
        RemoveLayer(top);
    }

    public void Flatten()
    {
        if (_layers.Count == 1) return;
        
        var first = _layers.Last();
        var finalImg = Image.Create(first.Width, first.Height, false, Image.Format.Rgba8);
        finalImg.Fill(Colors.Transparent);

        // Componer de abajo hacia arriba
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var l = _layers[i];
            if (!l.IsVisible) continue;
            
            var src = l.ImageData;
            for (int y = 0; y < src.GetHeight(); y++)
            {
                for (int x = 0; x < src.GetWidth(); x++)
                {
                    var cSrc = src.GetPixel(x, y);
                    if (cSrc.A > 0.001f)
                    {
                        var cDst = finalImg.GetPixel(x, y);
                        float a = cSrc.A + cDst.A * (1 - cSrc.A);
                        if (a > 0)
                        {
                            Vector3 rgb = (cSrc.Rgb * cSrc.A + cDst.Rgb * cDst.A * (1 - cSrc.A)) / a;
                            finalImg.SetPixel(x, y, new Color(rgb.X, rgb.Y, rgb.Z, a));
                        }
                    }
                }
            }
        }

        _layers.Clear();
        var flatLayer = new Layer();
        flatLayer.LayerName = "Capa Aplanada";
        flatLayer.Initialize(first.Width, first.Height, Colors.Transparent);
        
        for (int y = 0; y < finalImg.GetHeight(); y++)
            for (int x = 0; x < finalImg.GetWidth(); x++)
                flatLayer.ImageData.SetPixel(x, y, finalImg.GetPixel(x, y));
        
        flatLayer.UpdateTexture();
        _layers.Add(flatLayer);
        _activeLayer = flatLayer;
        LayerListChanged.Emit();
    }
    
    public void NotifyLayerUpdated(Layer layer)
    {
        layer.UpdateTexture();
    }
}
