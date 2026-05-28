using Godot;
using System;

namespace PhotoGodot.UI;

public partial class MainUI : Control
{
    [Export] private MenuButton _menuFile = null!;
    [Export] private MenuButton _menuEdit = null!;
    [Export] private MenuButton _menuLayer = null!;
    [Export] private MenuButton _menuView = null!;
    
    [Export] private Button _toolButtonBrush = null!;
    [Export] private Button _toolButtonEraser = null!;
    [Export] private Button _toolButtonPicker = null!;
    [Export] private Button _toolButtonMove = null!;
    [Export] private Button _toolButtonSelect = null!;
    
    [Export] private Label _currentToolLabel = null!;
    [Export] private ColorRect _primaryColorPreview = null!;
    [Export] private ColorPickerButton _colorPicker = null!;
    [Export] private HSlider _brushSizeSlider = null!;
    [Export] private HSlider _opacitySlider = null!;
    [Export] private HSlider _hardnessSlider = null!;
    
    [Export] private Button _newLayerButton = null!;
    [Export] private Button _deleteLayerButton = null!;
    [Export] private VBoxContainer _layersList = null!;
    
    [Export] private Button _grayscaleButton = null!;
    [Export] private Button _invertButton = null!;
    [Export] private Button _blurButton = null!;
    [Export] private Button _sharpenButton = null!;
    
    [Export] private Label _statusInfo = null!;
    [Export] private Label _zoomLabel = null!;
    
    private Core.LayerManager? _layerManager;
    private Core.ToolManager? _toolManager;
    private Core.HistoryManager? _historyManager;
    private Core.DrawingCanvas? _canvas;
    
    private FileDialog? _fileDialog;
    private int _canvasWidth = 1024;
    private int _canvasHeight = 768;

    public override void _Ready()
    {
        SetupMenus();
        SetupSignals();
        
        GD.Print("[MainUI] Ready");
    }

    public void Initialize(Core.LayerManager layerManager, Core.ToolManager toolManager, 
                          Core.HistoryManager historyManager, Core.DrawingCanvas canvas)
    {
        _layerManager = layerManager;
        _toolManager = toolManager;
        _historyManager = historyManager;
        _canvas = canvas;
        
        // Connect to events
        _toolManager.OnToolChanged += OnToolChanged;
        _toolManager.OnToolPropertiesChanged += UpdateToolProperties;
        _layerManager.OnLayersChanged += UpdateLayersList;
        _layerManager.OnActiveLayerChanged += UpdateLayersList;
        _canvas.OnZoomChanged += OnZoomChanged;
        
        // Initialize UI state
        UpdateToolProperties();
        UpdateLayersList();
        UpdateStatus("PhotoGodot Pro v1.0 - Listo");
    }

    private void SetupMenus()
    {
        if (_menuFile != null)
        {
            var popup = _menuFile.GetPopup();
            popup.IdPressed += OnMenuFilePressed;
        }
        
        if (_menuEdit != null)
        {
            var popup = _menuEdit.GetPopup();
            popup.IdPressed += OnMenuEditPressed;
        }
        
        if (_menuLayer != null)
        {
            var popup = _menuLayer.GetPopup();
            popup.IdPressed += OnMenuLayerPressed;
        }
        
        if (_menuView != null)
        {
            var popup = _menuView.GetPopup();
            popup.IdPressed += OnMenuViewPressed;
        }
    }

    private void SetupSignals()
    {
        if (_colorPicker != null)
        {
            _colorPicker.ColorChanged += OnColorChanged;
        }
        
        if (_brushSizeSlider != null)
        {
            _brushSizeSlider.ValueChanged += OnBrushSizeChanged;
        }
        
        if (_opacitySlider != null)
        {
            _opacitySlider.ValueChanged += OnOpacityChanged;
        }
        
        if (_hardnessSlider != null)
        {
            _hardnessSlider.ValueChanged += OnHardnessChanged;
        }
    }

    #region Tool Selection Methods
    
    public void OnSelectBrush() => _toolManager?.SelectTool("Brush");
    public void OnSelectEraser() => _toolManager?.SelectTool("Eraser");
    public void OnSelectColorPicker() => _toolManager?.SelectTool("ColorPicker");
    public void OnSelectMove() => _toolManager?.SelectTool("Move");
    public void OnSelectSelect() => _toolManager?.SelectTool("Select");
    
    #endregion

    #region Layer Methods
    
    public void OnNewLayer()
    {
        if (_layerManager == null || _historyManager == null) return;
        
        _historyManager.PushLayerAction("Add Layer", -1, "Added new layer");
        _layerManager.AddLayer($"Layer {_layerManager.LayerCount + 1}");
    }
    
    public void OnDeleteLayer()
    {
        if (_layerManager == null || _historyManager == null) return;
        
        var index = _layerManager.ActiveLayerIndex;
        if (index >= 0)
        {
            _historyManager.PushLayerAction("Delete Layer", index, "Deleted layer");
            _layerManager.DeleteLayer(index);
        }
    }
    
    #endregion

    #region Filter Methods
    
    public void OnApplyGrayscale()
    {
        ApplyFilterToActiveLayer("Grayscale", layer => layer.Grayscale());
    }
    
    public void OnApplyInvert()
    {
        ApplyFilterToActiveLayer("Invert", layer => layer.Invert());
    }
    
    public void OnApplyBlur()
    {
        ApplyFilterToActiveLayer("Blur", layer => layer.ApplyBlur(2));
    }
    
    public void OnApplySharpen()
    {
        ApplyFilterToActiveLayer("Sharpen", layer => layer.ApplySharpen());
    }
    
    private void ApplyFilterToActiveLayer(string filterName, Action<Core.Layer> filterAction)
    {
        if (_layerManager?.ActiveLayer == null || _historyManager == null) return;
        
        var layer = _layerManager.ActiveLayer;
        _historyManager.PushAction(filterName, layer, _layerManager.ActiveLayerIndex, $"Applied {filterName} filter");
        filterAction(layer);
        
        UpdateStatus($"{filterName} aplicado");
    }
    
    #endregion

    #region Menu Handlers
    
    private void OnMenuFilePressed(int id)
    {
        switch (id)
        {
            case 0: // Nuevo
                NewProject();
                break;
            case 1: // Abrir
                OpenProject();
                break;
            case 2: // Guardar
                SaveProject();
                break;
            case 3: // Exportar PNG
                ExportImage("png");
                break;
            case 4: // Exportar JPG
                ExportImage("jpg");
                break;
        }
    }
    
    private void OnMenuEditPressed(int id)
    {
        switch (id)
        {
            case 0: // Deshacer
                _toolManager?.Undo();
                UpdateStatus("Deshacer");
                break;
            case 1: // Rehacer
                _toolManager?.Redo();
                UpdateStatus("Rehacer");
                break;
        }
    }
    
    private void OnMenuLayerPressed(int id)
    {
        switch (id)
        {
            case 0: // Nueva Capa
                OnNewLayer();
                break;
            case 1: // Duplicar Capa
                DuplicateActiveLayer();
                break;
            case 2: // Eliminar Capa
                OnDeleteLayer();
                break;
            case 3: // Fusionar hacia abajo
                MergeDown();
                break;
            case 4: // Aplanar imagen
                FlattenImage();
                break;
        }
    }
    
    private void OnMenuViewPressed(int id)
    {
        switch (id)
        {
            case 0: // Mostrar Grid
                _canvas?.ToggleGrid();
                break;
            case 1: // Zoom In
                _canvas?.ZoomIn();
                break;
            case 2: // Zoom Out
                _canvas?.ZoomOut();
                break;
            case 3: // Ajustar a ventana
                _canvas?.FitToWindow();
                break;
        }
    }
    
    #endregion

    #region Event Handlers
    
    private void OnToolChanged(Core.BaseTool? tool)
    {
        if (_currentToolLabel != null && tool != null)
        {
            _currentToolLabel.Text = tool.Name;
        }
        
        UpdateToolButtons(tool);
    }
    
    private void UpdateToolButtons(Core.BaseTool? tool)
    {
        if (_toolButtonBrush != null) _toolButtonBrush.ButtonPressed = tool is PhotoGodot.Tools.BrushTool;
        if (_toolButtonEraser != null) _toolButtonEraser.ButtonPressed = tool is PhotoGodot.Tools.EraserTool;
        if (_toolButtonPicker != null) _toolButtonPicker.ButtonPressed = tool is PhotoGodot.Tools.ColorPickerTool;
        if (_toolButtonMove != null) _toolButtonMove.ButtonPressed = tool is PhotoGodot.Tools.MoveTool;
        if (_toolButtonSelect != null) _toolButtonSelect.ButtonPressed = tool is PhotoGodot.Tools.SelectTool;
    }
    
    private void UpdateToolProperties()
    {
        if (_toolManager == null) return;
        
        if (_primaryColorPreview != null)
        {
            _primaryColorPreview.Color = _toolManager.PrimaryColor;
        }
        
        if (_colorPicker != null)
        {
            _colorPicker.Color = _toolManager.PrimaryColor;
        }
        
        if (_brushSizeSlider != null)
        {
            _brushSizeSlider.Value = _toolManager.BrushSize;
        }
        
        if (_opacitySlider != null)
        {
            _opacitySlider.Value = _toolManager.Opacity * 100;
        }
        
        if (_hardnessSlider != null)
        {
            _hardnessSlider.Value = _toolManager.Hardness * 100;
        }
    }
    
    private void OnColorChanged(Color color)
    {
        _toolManager?.SetPrimaryColor(color);
    }
    
    private void OnBrushSizeChanged(double value)
    {
        _toolManager?.SetBrushSize((float)value);
    }
    
    private void OnOpacityChanged(double value)
    {
        _toolManager?.SetOpacity((float)value / 100);
    }
    
    private void OnHardnessChanged(double value)
    {
        _toolManager?.SetHardness((float)value / 100);
    }
    
    private void OnZoomChanged(float zoom)
    {
        if (_zoomLabel != null)
        {
            _zoomLabel.Text = $"{(int)(zoom * 100)}%";
        }
    }
    
    private void UpdateLayersList()
    {
        if (_layersList == null || _layerManager == null) return;
        
        // Clear existing layer items
        foreach (var child in _layersList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add layer items
        var layers = _layerManager.GetAllLayers();
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            var layer = layers[i];
            var button = new Button
            {
                Text = $"{(layer.Visible ? "👁" : "🚫")} {layer.Name}",
                ToggleMode = true,
                ButtonPressed = i == _layerManager.ActiveLayerIndex
            };
            
            int layerIndex = i;
            button.Pressed += () => OnLayerSelected(layerIndex);
            
            _layersList.AddChild(button);
        }
    }
    
    private void OnLayerSelected(int index)
    {
        if (_layerManager != null)
        {
            _layerManager.ActiveLayerIndex = index;
        }
    }
    
    #endregion

    #region File Operations
    
    private void NewProject()
    {
        _canvasWidth = 1024;
        _canvasHeight = 768;
        
        if (_layerManager != null && _canvas != null)
        {
            _layerManager.Initialize(_canvasWidth, _canvasHeight);
            _canvas.SetCanvasSize(_canvasWidth, _canvasHeight);
            _historyManager?.Clear();
            
            UpdateStatus($"Nuevo proyecto: {_canvasWidth}x{_canvasHeight}");
            UpdateLayersList();
        }
    }
    
    private void OpenProject()
    {
        UpdateStatus("Abrir proyecto - No implementado en demo");
    }
    
    private void SaveProject()
    {
        if (_layerManager == null) return;
        
        var data = _layerManager.SaveProject();
        GD.Print($"[MainUI] Project saved: {data.Length} bytes");
        
        UpdateStatus("Proyecto guardado");
    }
    
    private void ExportImage(string format)
    {
        if (_layerManager == null) return;
        
        var composited = _layerManager.GetCompositedImage();
        byte[] data;
        
        switch (format.ToLower())
        {
            case "jpg":
                data = composited.SaveJpgToBuffer(90);
                break;
            case "webp":
                data = composited.SaveWebpToBuffer(90);
                break;
            default:
                data = composited.SavePngToBuffer();
                break;
        }
        
        GD.Print($"[MainUI] Exported {format.ToUpper()}: {data.Length} bytes");
        UpdateStatus($"Exportado como {format.ToUpper()}");
    }
    
    private void DuplicateActiveLayer()
    {
        if (_layerManager == null || _historyManager == null) return;
        
        var index = _layerManager.ActiveLayerIndex;
        if (index >= 0)
        {
            _historyManager.PushLayerAction("Duplicate Layer", index, "Duplicated layer");
            _layerManager.DuplicateLayer(index);
        }
    }
    
    private void MergeDown()
    {
        if (_layerManager == null || _historyManager == null) return;
        
        var index = _layerManager.ActiveLayerIndex;
        if (index > 0)
        {
            _historyManager.PushLayerAction("Merge Down", index, "Merged layer down");
            _layerManager.MergeDown(index);
        }
    }
    
    private void FlattenImage()
    {
        if (_layerManager == null || _historyManager == null) return;
        
        _historyManager.PushLayerAction("Flatten Image", 0, "Flattened image");
        _layerManager.FlattenImage();
    }
    
    #endregion

    private void UpdateStatus(string message)
    {
        if (_statusInfo != null)
        {
            _statusInfo.Text = message;
        }
        GD.Print($"[Status] {message}");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Global shortcuts
            if (keyEvent.Keycode == Key.Z && keyEvent.CtrlPressed)
            {
                if (keyEvent.ShiftPressed)
                {
                    _toolManager?.Redo();
                }
                else
                {
                    _toolManager?.Undo();
                }
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.Y && keyEvent.CtrlPressed)
            {
                _toolManager?.Redo();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.N && keyEvent.CtrlPressed)
            {
                NewProject();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.S && keyEvent.CtrlPressed)
            {
                SaveProject();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
