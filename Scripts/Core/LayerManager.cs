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
    private Node2D _renderContainer;

    public int LayerCount => _layers.Count;
    public Layer ActiveLayer => _activeLayer;
    public IReadOnlyList<Layer> Layers => _layers.AsReadOnly();

    public override void _Ready()
    {
        _renderContainer = new Node2D();
        _renderContainer.Name = "RenderContainer";
        AddChild(_renderContainer);
    }

    public void Setup(int width, int height, Color bg)
    {
        CreateLayer("Fondo", bg);
    }

    public Layer CreateLayer(string name, Color? fillColor = null)
    {
        var layer = new Layer();
        layer.LayerName = name;
        int w = _layers.Count > 0 ? _layers[0].Width : 1920;
        int h = _layers.Count > 0 ? _layers[0].Height : 1080;
        
        layer.Initialize(w, h, fillColor ?? Colors.Transparent);
        
        _layers.Insert(0, layer);
        if (_activeLayer == null) _activeLayer = layer;

        RebuildRenderTree();
        LayerListChanged.Emit();
        ActiveLayerChanged.Emit(_activeLayer);
        return layer;
    }

    public void RemoveLayer(Layer layer)
    {
        if (_layers.Count <= 1) return;
        
        int idx = _layers.IndexOf(layer);
        _layers.Remove(layer);
        
        if (_activeLayer == layer)
        {
            _activeLayer = _layers[System.Math.Max(0, idx - 1)];
            ActiveLayerChanged.Emit(_activeLayer);
        }
        
        RebuildRenderTree();
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

    public void MoveLayer(int index, int newIndex)
    {
        if (index < 0 || index >= _layers.Count || newIndex < 0 || newIndex >= _layers.Count) return;
        var layer = _layers[index];
        _layers.RemoveAt(index);
        _layers.Insert(newIndex, layer);
        RebuildRenderTree();
        LayerListChanged.Emit();
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
                if (cTop.A > 0)
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
        
        var bottom = _layers.Last();
        var finalImg = Image.Create(bottom.Width, bottom.Height, false, Image.Format.Rgba8);
        finalImg.Fill(Colors.Transparent);

        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var l = _layers[i];
            if (!l.IsVisible) continue;
            var src = l.ImageData;
            for(int y=0; y<src.GetHeight(); y++)
            {
                for(int x=0; x<src.GetWidth(); x++)
                {
                    var cSrc = src.GetPixel(x,y);
                    if(cSrc.A > 0)
                    {
                        var cDst = finalImg.GetPixel(x,y);
                        float a = cSrc.A + cDst.A * (1 - cSrc.A);
                        if(a>0){
                            Vector3 rgb = (cSrc.Rgb * cSrc.A + cDst.Rgb * cDst.A * (1 - cSrc.A)) / a;
                            finalImg.SetPixel(x,y, new Color(rgb.X, rgb.Y, rgb.Z, a));
                        }
                    }
                }
            }
        }

        _layers.Clear();
        var flatLayer = new Layer();
        flatLayer.LayerName = "Capa Aplanada";
        flatLayer.Initialize(bottom.Width, bottom.Height, Colors.Transparent);
        for(int y=0; y<finalImg.GetHeight(); y++)
            for(int x=0; x<finalImg.GetWidth(); x++)
                flatLayer.ImageData.SetPixel(x,y, finalImg.GetPixel(x,y));
        
        flatLayer.UpdateTexture();
        _layers.Add(flatLayer);
        _activeLayer = flatLayer;
        RebuildRenderTree();
        LayerListChanged.Emit();
    }

    private void RebuildRenderTree()
    {
        foreach (var child in _renderContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];
            var sprite = new Sprite2D();
            sprite.Texture = layer.Texture;
            sprite.Modulate = new Color(1, 1, 1, layer.Opacity);
            _renderContainer.AddChild(sprite);
        }
    }
    
    public void NotifyLayerUpdated(Layer layer)
    {
        layer.UpdateTexture();
    }
    
    public void ClearAll()
    {
        _layers.Clear();
        _activeLayer = null;
        foreach (var child in _renderContainer.GetChildren())
        {
            child.QueueFree();
        }
        LayerListChanged.Emit();
    }
}
