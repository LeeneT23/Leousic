using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestiona todas las capas del proyecto.
/// Permite crear, eliminar, reordenar y modificar capas.
/// </summary>
public partial class LayerManager : Node
{
    [Signal] public delegate void LayerAddedEventHandler(Layer layer);
    [Signal] public delegate void LayerRemovedEventHandler(int layerId);
    [Signal] public delegate void LayerModifiedEventHandler(int layerId);
    [Signal] public delegate void ActiveLayerChangedEventHandler(int layerId);
    [Signal] public delegate void LayersReorderedEventHandler();
    
    private List<Layer> _layers = new();
    private int _activeLayerId = -1;
    private int _nextLayerId = 1;
    
    [Export] public int MaxLayers { get; set; } = 100;
    [Export] public DrawingCanvas? Canvas { get; set; }
    
    public int LayerCount => _layers.Count;
    public int ActiveLayerId => _activeLayerId;
    
    public override void _Ready()
    {
        GD.Print("[LayerManager] Inicializado");
    }
    
    /// <summary>
    /// Crea una nueva capa vacía
    /// </summary>
    public Layer CreateLayer(string? name = null)
    {
        if (_layers.Count >= MaxLayers)
        {
            GD.PrintErr($"[LayerManager] Máximo de capas alcanzado ({MaxLayers})");
            return null!;
        }
        
        string layerName = name ?? $"Capa {_nextLayerId}";
        var layer = Layer.Create(_nextLayerId, layerName, Canvas?.CanvasWidth ?? 1920, Canvas?.CanvasHeight ?? 1080);
        
        _layers.Add(layer);
        int layerId = _nextLayerId++;
        
        // Activar la nueva capa
        SetActiveLayer(layerId);
        
        EmitSignal(SignalName.LayerAdded, layer);
        UpdateCanvasComposite();
        
        GD.Print($"[LayerManager] Capa creada: {layerName} (ID: {layerId})");
        return layer;
    }
    
    /// <summary>
    /// Crea una capa desde una imagen
    /// </summary>
    public Layer CreateLayerFromImage(Image image, string? name = null)
    {
        if (_layers.Count >= MaxLayers)
        {
            GD.PrintErr($"[LayerManager] Máximo de capas alcanzado ({MaxLayers})");
            return null!;
        }
        
        string layerName = name ?? $"Imagen {_nextLayerId}";
        var layer = Layer.CreateFromImage(_nextLayerId, layerName, image);
        
        _layers.Add(layer);
        int layerId = _nextLayerId++;
        
        SetActiveLayer(layerId);
        
        EmitSignal(SignalName.LayerAdded, layer);
        UpdateCanvasComposite();
        
        GD.Print($"[LayerManager] Capa creada desde imagen: {layerName} (ID: {layerId})");
        return layer;
    }
    
    /// <summary>
    /// Elimina una capa por ID
    /// </summary>
    public bool RemoveLayer(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer == null)
        {
            GD.PrintErr($"[LayerManager] Capa no encontrada: {layerId}");
            return false;
        }
        
        _layers.Remove(layer);
        
        if (_activeLayerId == layerId)
        {
            _activeLayerId = _layers.Count > 0 ? _layers[_layers.Count - 1].Id : -1;
        }
        
        EmitSignal(SignalName.LayerRemoved, layerId);
        UpdateCanvasComposite();
        
        GD.Print($"[LayerManager] Capa eliminada: {layerId}");
        return true;
    }
    
    /// <summary>
    /// Elimina la capa activa
    /// </summary>
    public bool RemoveActiveLayer()
    {
        if (_activeLayerId == -1)
        {
            GD.PrintErr("[LayerManager] No hay capa activa");
            return false;
        }
        
        return RemoveLayer(_activeLayerId);
    }
    
    /// <summary>
    /// Obtiene una capa por ID
    /// </summary>
    public Layer? GetLayer(int layerId)
    {
        return _layers.Find(l => l.Id == layerId);
    }
    
    /// <summary>
    /// Obtiene la capa activa
    /// </summary>
    public Layer? GetActiveLayer()
    {
        return GetLayer(_activeLayerId);
    }
    
    /// <summary>
    /// Obtiene todas las capas en orden
    /// </summary>
    public List<Layer> GetAllLayers()
    {
        return new List<Layer>(_layers);
    }
    
    /// <summary>
    /// Establece la capa activa
    /// </summary>
    public void SetActiveLayer(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer == null)
        {
            GD.PrintErr($"[LayerManager] Capa no encontrada: {layerId}");
            return;
        }
        
        _activeLayerId = layerId;
        EmitSignal(SignalName.ActiveLayerChanged, layerId);
        GD.Print($"[LayerManager] Capa activa: {layer.Name}");
    }
    
    /// <summary>
    /// Cambia la visibilidad de una capa
    /// </summary>
    public void ToggleLayerVisibility(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer != null)
        {
            layer.Visible = !layer.Visible;
            EmitSignal(SignalName.LayerModified, layerId);
            UpdateCanvasComposite();
        }
    }
    
    /// <summary>
    /// Bloquea/desbloquea una capa
    /// </summary>
    public void ToggleLayerLock(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer != null)
        {
            layer.Locked = !layer.Locked;
            EmitSignal(SignalName.LayerModified, layerId);
        }
    }
    
    /// <summary>
    /// Duplica una capa
    /// </summary>
    public Layer? DuplicateLayer(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer == null)
        {
            GD.PrintErr($"[LayerManager] Capa no encontrada: {layerId}");
            return null;
        }
        
        var duplicate = layer.Duplicate();
        int index = _layers.IndexOf(layer);
        _layers.Insert(index + 1, duplicate);
        
        SetActiveLayer(duplicate.Id);
        
        EmitSignal(SignalName.LayerAdded, duplicate);
        UpdateCanvasComposite();
        
        GD.Print($"[LayerManager] Capa duplicada: {layer.Name} -> {duplicate.Name}");
        return duplicate;
    }
    
    /// <summary>
    /// Mueve una capa arriba en el orden
    /// </summary>
    public bool MoveLayerUp(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer == null)
            return false;
        
        int index = _layers.IndexOf(layer);
        if (index < _layers.Count - 1)
        {
            _layers.RemoveAt(index);
            _layers.Insert(index + 1, layer);
            
            EmitSignal(SignalName.LayersReordered);
            UpdateCanvasComposite();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Mueve una capa abajo en el orden
    /// </summary>
    public bool MoveLayerDown(int layerId)
    {
        var layer = GetLayer(layerId);
        if (layer == null)
            return false;
        
        int index = _layers.IndexOf(layer);
        if (index > 0)
        {
            _layers.RemoveAt(index);
            _layers.Insert(index - 1, layer);
            
            EmitSignal(SignalName.LayersReordered);
            UpdateCanvasComposite();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Fusiona la capa activa con la capa inferior
    /// </summary>
    public bool MergeWithLayerBelow()
    {
        if (_activeLayerId == -1)
            return false;
        
        var activeLayer = GetActiveLayer();
        if (activeLayer == null)
            return false;
        
        int index = _layers.IndexOf(activeLayer);
        if (index <= 0)
        {
            GD.Print("[LayerManager] No hay capa debajo para fusionar");
            return false;
        }
        
        var layerBelow = _layers[index - 1];
        
        // Fusionar imágenes
        Image merged = layerBelow.GetImage().Duplicate();
        Image activeImage = activeLayer.GetImage();
        
        for (int y = 0; y < merged.GetSize().Y; y++)
        {
            for (int x = 0; x < merged.GetSize().X; x++)
            {
                Color below = merged.GetPixel(x, y);
                Color above = activeImage.GetPixel(x, y);
                
                if (above.A > 0)
                {
                    float alpha = above.A;
                    Color blended = new Color(
                        below.R * (1 - alpha) + above.R * alpha,
                        below.G * (1 - alpha) + above.G * alpha,
                        below.B * (1 - alpha) + above.B * alpha,
                        Mathf.Max(below.A, above.A)
                    );
                    merged.SetPixel(x, y, blended);
                }
            }
        }
        
        // Actualizar capa inferior
        layerBelow.UpdateTexture(merged);
        
        // Eliminar capa activa
        RemoveLayer(_activeLayerId);
        
        GD.Print("[LayerManager] Capas fusionadas");
        return true;
    }
    
    /// <summary>
    /// Aplana todas las capas visibles en una sola
    /// </summary>
    public Layer? FlattenLayers()
    {
        if (_layers.Count == 0)
            return null;
        
        Image flattened = Image.CreateEmpty(Canvas?.CanvasWidth ?? 1920, Canvas?.CanvasHeight ?? 1080, false, Image.Format.Rgba8);
        flattened.Fill(Colors.White);
        
        foreach (var layer in _layers)
        {
            if (layer.Visible && layer.Texture != null)
            {
                Image layerImage = layer.Texture.GetImage();
                
                for (int y = 0; y < layerImage.GetSize().Y; y++)
                {
                    for (int x = 0; x < layerImage.GetSize().X; x++)
                    {
                        Color pixel = layerImage.GetPixel(x, y);
                        if (pixel.A > 0)
                        {
                            int targetX = x + (int)layer.Offset.X;
                            int targetY = y + (int)layer.Offset.Y;
                            
                            if (targetX >= 0 && targetX < flattened.GetSize().X && 
                                targetY >= 0 && targetY < flattened.GetSize().Y)
                            {
                                Color existing = flattened.GetPixel(targetX, targetY);
                                float alpha = pixel.A;
                                Color blended = new Color(
                                    existing.R * (1 - alpha) + pixel.R * alpha,
                                    existing.G * (1 - alpha) + pixel.G * alpha,
                                    existing.B * (1 - alpha) + pixel.B * alpha,
                                    Mathf.Max(existing.A, pixel.A)
                                );
                                flattened.SetPixel(targetX, targetY, blended);
                            }
                        }
                    }
                }
            }
        }
        
        // Limpiar capas existentes
        _layers.Clear();
        
        // Crear nueva capa aplanada
        var flatLayer = Layer.CreateFromImage(_nextLayerId++, "Fondo", flattened);
        _layers.Add(flatLayer);
        SetActiveLayer(flatLayer.Id);
        
        EmitSignal(SignalName.LayerAdded, flatLayer);
        UpdateCanvasComposite();
        
        GD.Print("[LayerManager] Todas las capas aplanadas");
        return flatLayer;
    }
    
    /// <summary>
    /// Marca una capa como modificada
    /// </summary>
    public void MarkLayerAsModified(int layerId)
    {
        EmitSignal(SignalName.LayerModified, layerId);
    }
    
    /// <summary>
    /// Actualiza la composición del canvas
    /// </summary>
    private void UpdateCanvasComposite()
    {
        Canvas?.CallDeferred(nameof(DrawingCanvas.UpdateComposite));
    }
    
    /// <summary>
    /// Limpia todas las capas
    /// </summary>
    public void ClearAllLayers()
    {
        _layers.Clear();
        _activeLayerId = -1;
        _nextLayerId = 1;
        
        GD.Print("[LayerManager] Todas las capas eliminadas");
    }
}
