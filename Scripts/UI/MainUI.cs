using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// UI Principal de PhotoGodot Pro.
/// Gestiona todos los elementos de la interfaz de usuario.
/// </summary>
public partial class MainUI : Control
{
    [Signal] public delegate void NewDocumentRequestedEventHandler(int width, int height);
    [Signal] public delegate void OpenFileRequestedEventHandler(string path);
    [Signal] public delegate void SaveRequestedEventHandler();
    [Signal] public delegate void ExportRequestedEventHandler(string path, string format);
    
    // Referencias a componentes principales
    private ToolManager? _toolManager;
    private LayerManager? _layerManager;
    private HistoryManager? _historyManager;
    private DrawingCanvas? _canvas;
    
    // Elementos de UI
    private ColorPicker? _colorPicker;
    private HSlider? _brushSizeSlider;
    private HSlider? _opacitySlider;
    private HSlider? _hardnessSlider;
    private Label? _currentToolLabel;
    private Label? _zoomLabel;
    private VBoxContainer? _layersList;
    private ColorRect? _primaryColorPreview;
    private ColorRect? _secondaryColorPreview;
    
    // Colores
    private Color _primaryColor = Colors.Black;
    private Color _secondaryColor = Colors.White;
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        
        GD.Print("[MainUI] UI inicializada");
    }
    
    /// <summary>
    /// Configura todos los elementos de la UI
    /// </summary>
    private void SetupUI()
    {
        // Obtener referencias a los elementos de UI
        _colorPicker = GetNodeOrNull<ColorPicker>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/ColorPicker");
        _brushSizeSlider = GetNodeOrNull<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/BrushSizeSlider");
        _opacitySlider = GetNodeOrNull<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/OpacitySlider");
        _hardnessSlider = GetNodeOrNull<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/HardnessSlider");
        _currentToolLabel = GetNodeOrNull<Label>("MarginContainer/VBoxContainer/TopPanel/ToolBar/CurrentToolLabel");
        _zoomLabel = GetNodeOrNull<Label>("MarginContainer/VBoxContainer/StatusBar/ZoomLabel");
        _layersList = GetNodeOrNull<VBoxContainer>("MarginContainer/VBoxContainer/RightPanel/LayersPanel/LayersList");
        _primaryColorPreview = GetNodeOrNull<ColorRect>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/PrimaryColorPreview");
        _secondaryColorPreview = GetNodeOrNull<ColorRect>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/SecondaryColorPreview");
        
        // Configurar valores iniciales
        if (_brushSizeSlider != null)
            _brushSizeSlider.Value = 10;
        if (_opacitySlider != null)
            _opacitySlider.Value = 100;
        if (_hardnessSlider != null)
            _hardnessSlider.Value = 100;
        
        // Configurar color picker
        if (_colorPicker != null)
        {
            _colorPicker.Color = _primaryColor;
        }
        
        UpdateColorPreviews();
    }
    
    /// <summary>
    /// Conecta las señales de la UI
    /// </summary>
    private void ConnectSignals()
    {
        // Conectar sliders
        if (_brushSizeSlider != null)
            _brushSizeSlider.ValueChanged += OnBrushSizeChanged;
        if (_opacitySlider != null)
            _opacitySlider.ValueChanged += OnOpacityChanged;
        if (_hardnessSlider != null)
            _hardnessSlider.ValueChanged += OnHardnessChanged;
        
        // Conectar color picker
        if (_colorPicker != null)
            _colorPicker.ColorChanged += OnColorChanged;
    }
    
    /// <summary>
    /// Establece las referencias a los managers
    /// </summary>
    public void SetManagers(ToolManager toolManager, LayerManager layerManager, HistoryManager historyManager, DrawingCanvas canvas)
    {
        _toolManager = toolManager;
        _layerManager = layerManager;
        _historyManager = historyManager;
        _canvas = canvas;
        
        // Conectar señales del canvas
        if (_canvas != null)
        {
            _canvas.ZoomChanged += OnZoomChanged;
        }
        
        // Crear primera capa por defecto
        _layerManager?.CreateLayer("Fondo");
        
        // Actualizar lista de capas
        UpdateLayersList();
    }
    
    /// <summary>
    /// Se llama cuando se registra una nueva herramienta
    /// </summary>
    public void OnToolRegistered(BaseTool tool)
    {
        // La herramienta se puede agregar al toolbar si es necesario
        GD.Print($"[MainUI] Herramienta registrada: {tool.ToolName}");
    }
    
    /// <summary>
    /// Se llama cuando se selecciona un color con el cuentagotas
    /// </summary>
    public void OnColorPicked(Color color)
    {
        _primaryColor = color;
        
        if (_colorPicker != null)
            _colorPicker.Color = color;
        
        UpdateColorPreviews();
        UpdateToolSettings();
    }
    
    private void OnBrushSizeChanged(double value)
    {
        UpdateToolSettings();
    }
    
    private void OnOpacityChanged(double value)
    {
        UpdateToolSettings();
    }
    
    private void OnHardnessChanged(double value)
    {
        UpdateToolSettings();
    }
    
    private void OnColorChanged(Color color)
    {
        _primaryColor = color;
        UpdateColorPreviews();
        UpdateToolSettings();
    }
    
    private void OnZoomChanged(float zoom)
    {
        if (_zoomLabel != null)
        {
            _zoomLabel.Text = $"{(int)(zoom * 100)}%";
        }
    }
    
    /// <summary>
    /// Actualiza los ajustes de la herramienta actual
    /// </summary>
    private void UpdateToolSettings()
    {
        if (_toolManager == null)
            return;
        
        var settings = new Dictionary<string, Variant>
        {
            { "BrushSize", (float)(_brushSizeSlider?.Value ?? 10) },
            { "Opacity", (float)((_opacitySlider?.Value ?? 100) / 100.0) },
            { "BrushHardness", (float)((_hardnessSlider?.Value ?? 100) / 100.0) },
            { "PrimaryColor", _primaryColor }
        };
        
        _toolManager.UpdateCurrentToolSettings(settings);
    }
    
    /// <summary>
    /// Actualiza las vistas previas de color
    /// </summary>
    private void UpdateColorPreviews()
    {
        if (_primaryColorPreview != null)
            _primaryColorPreview.Color = _primaryColor;
        if (_secondaryColorPreview != null)
            _secondaryColorPreview.Color = _secondaryColor;
    }
    
    /// <summary>
    /// Actualiza la lista de capas en el panel
    /// </summary>
    private void UpdateLayersList()
    {
        if (_layersList == null || _layerManager == null)
            return;
        
        // Limpiar lista actual
        foreach (var child in _layersList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Agregar capas en orden inverso (la superior primero)
        var layers = _layerManager.GetAllLayers();
        layers.Reverse();
        
        foreach (var layer in layers)
        {
            CreateLayerListItem(layer);
        }
    }
    
    /// <summary>
    /// Crea un elemento de lista para una capa
    /// </summary>
    private void CreateLayerListItem(Layer layer)
    {
        if (_layersList == null)
            return;
        
        var hbox = new HBoxContainer();
        
        // Checkbox de visibilidad
        var visibilityCheck = new CheckBox();
        visibilityCheck.ButtonPressed = layer.Visible;
        visibilityCheck.Toggled += (pressed) => _layerManager?.ToggleLayerVisibility(layer.Id);
        hbox.AddChild(visibilityCheck);
        
        // Nombre de la capa
        var nameLabel = new Label();
        nameLabel.Text = layer.Name;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(nameLabel);
        
        // Botón de eliminar
        var deleteButton = new Button();
        deleteButton.Text = "🗑";
        deleteButton.Pressed += () => _layerManager?.RemoveLayer(layer.Id);
        hbox.AddChild(deleteButton);
        
        _layersList.AddChild(hbox);
    }
    
    #region Menu Actions
    
    public void OnNewDocument()
    {
        GD.Print("[MainUI] Nuevo documento solicitado");
        EmitSignal(SignalName.NewDocumentRequested, 1920, 1080);
    }
    
    public void OnOpenFile()
    {
        GD.Print("[MainUI] Abrir archivo solicitado");
        // En una implementación real, abriría un diálogo de archivos
    }
    
    public void OnSave()
    {
        GD.Print("[MainUI] Guardar solicitado");
        EmitSignal(SignalName.SaveRequested);
    }
    
    public void OnSaveAs()
    {
        GD.Print("[MainUI] Guardar como solicitado");
    }
    
    public void OnExportPNG()
    {
        GD.Print("[MainUI] Exportar PNG solicitado");
        EmitSignal(SignalName.ExportRequested, "export.png", "png");
    }
    
    public void OnExportJPG()
    {
        GD.Print("[MainUI] Exportar JPG solicitado");
        EmitSignal(SignalName.ExportRequested, "export.jpg", "jpg");
    }
    
    public void OnUndo()
    {
        _historyManager?.Undo();
    }
    
    public void OnRedo()
    {
        _historyManager?.Redo();
    }
    
    public void OnNewLayer()
    {
        _layerManager?.CreateLayer();
        UpdateLayersList();
    }
    
    public void OnDuplicateLayer()
    {
        if (_layerManager != null)
        {
            _layerManager.DuplicateLayer(_layerManager.ActiveLayerId);
            UpdateLayersList();
        }
    }
    
    public void OnDeleteLayer()
    {
        if (_layerManager != null)
        {
            _layerManager.RemoveActiveLayer();
            UpdateLayersList();
        }
    }
    
    public void OnMergeDown()
    {
        if (_layerManager != null)
        {
            _layerManager.MergeWithLayerBelow();
            UpdateLayersList();
        }
    }
    
    public void OnFlattenImage()
    {
        if (_layerManager != null)
        {
            _layerManager.FlattenLayers();
            UpdateLayersList();
        }
    }
    
    public void OnToggleGrid()
    {
        if (_canvas != null)
        {
            _canvas.ShowGrid = !_canvas.ShowGrid;
            _canvas.QueueRedraw();
        }
    }
    
    public void OnZoomIn()
    {
        if (_canvas != null)
            _canvas.SetZoom(_canvas.CurrentZoom + 0.1f);
    }
    
    public void OnZoomOut()
    {
        if (_canvas != null)
            _canvas.SetZoom(_canvas.CurrentZoom - 0.1f);
    }
    
    public void OnFitToView()
    {
        _canvas?.FitToView();
    }
    
    public void OnSelectBrush()
    {
        _toolManager?.ActivateTool("BrushTool");
        if (_currentToolLabel != null)
            _currentToolLabel.Text = "Pincel";
    }
    
    public void OnSelectEraser()
    {
        _toolManager?.ActivateTool("EraserTool");
        if (_currentToolLabel != null)
            _currentToolLabel.Text = "Borrador";
    }
    
    public void OnSelectColorPicker()
    {
        _toolManager?.ActivateTool("ColorPickerTool");
        if (_currentToolLabel != null)
            _currentToolLabel.Text = "Selector";
    }
    
    public void OnSelectMove()
    {
        _toolManager?.ActivateTool("MoveTool");
        if (_currentToolLabel != null)
            _currentToolLabel.Text = "Mover";
    }
    
    public void OnSelectSelect()
    {
        _toolManager?.ActivateTool("SelectTool");
        if (_currentToolLabel != null)
            _currentToolLabel.Text = "Selección";
    }
    
    public void OnApplyGrayscale()
    {
        var activeLayer = _layerManager?.GetActiveLayer();
        activeLayer?.ApplyFilter("grayscale");
    }
    
    public void OnApplyInvert()
    {
        var activeLayer = _layerManager?.GetActiveLayer();
        activeLayer?.ApplyFilter("invert");
    }
    
    public void OnApplyBlur()
    {
        var activeLayer = _layerManager?.GetActiveLayer();
        activeLayer?.ApplyFilter("blur");
    }
    
    public void OnApplySharpen()
    {
        var activeLayer = _layerManager?.GetActiveLayer();
        activeLayer?.ApplyFilter("sharpen");
    }
    
    #endregion
}
