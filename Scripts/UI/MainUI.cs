using Godot;
using System;
using PhotoGodot.Core;

namespace PhotoGodot.UI;

public partial class MainUI : CanvasLayer
{
    private Main _main;
    
    // Toolbar superior
    private HBoxContainer _toolbar;
    private OptionButton _toolSelector;
    private ColorPickerButton _colorPicker;
    private SpinBox _brushSizeSlider;
    private SpinBox _opacitySlider;
    
    // Panel lateral de capas
    private VBoxContainer _layersPanel;
    private ItemList _layerList;
    private Button _newLayerBtn;
    private Button _deleteLayerBtn;
    private Button _duplicateLayerBtn;
    private Button _mergeDownBtn;
    private Button _flattenBtn;
    
    // Barra de estado inferior
    private Label _statusLabel;
    
    public override void _Ready()
    {
        Layer = 10; // Renderizar encima de todo
    }

    public void Setup(Main main)
    {
        _main = main;
        BuildUI();
        ConnectSignals();
    }

    private void BuildUI()
    {
        var mainScreen = _main.GetParent<Node>();
        
        // Crear contenedor principal tipo "border" con VBox
        var mainContainer = new VBoxContainer();
        mainScreen.AddChild(mainContainer);
        
        // === TOOLBAR SUPERIOR ===
        _toolbar = new HBoxContainer();
        _toolbar.AddThemeConstantOverride("separation", 10);
        mainContainer.AddChild(_toolbar);
        
        // Selector de herramientas
        var toolLabel = new Label();
        toolLabel.Text = "Herramienta:";
        _toolbar.AddChild(toolLabel);
        
        _toolSelector = new OptionButton();
        _toolSelector.AddItem("Pincel (B)", 0);
        _toolSelector.AddItem("Borrador (E)", 1);
        _toolSelector.AddItem("Selector Color (I)", 2);
        _toolSelector.AddItem("Mover (V)", 3);
        _toolSelector.AddItem("Selección (M)", 4);
        _toolSelector.ItemSelected += OnToolSelected;
        _toolbar.AddChild(_toolSelector);
        
        // Selector de color
        var colorLabel = new Label();
        colorLabel.Text = "Color:";
        _toolbar.AddChild(colorLabel);
        
        _colorPicker = new ColorPickerButton();
        _colorPicker.Color = Colors.Black;
        _colorPicker.ColorChanged += OnColorChanged;
        _toolbar.AddChild(_colorPicker);
        
        // Tamaño de pincel
        var sizeLabel = new Label();
        sizeLabel.Text = "Tamaño:";
        _toolbar.AddChild(sizeLabel);
        
        _brushSizeSlider = new SpinBox();
        _brushSizeSlider.MinValue = 1;
        _brushSizeSlider.MaxValue = 500;
        _brushSizeSlider.Step = 1;
        _brushSizeSlider.Value = 10;
        _brushSizeSlider.CustomMinimumSize = new Vector2(80, 0);
        _brushSizeSlider.ValueChanged += OnBrushSizeChanged;
        _toolbar.AddChild(_brushSizeSlider);
        
        // Opacidad
        var opacityLabel = new Label();
        opacityLabel.Text = "Opacidad:";
        _toolbar.AddChild(opacityLabel);
        
        _opacitySlider = new SpinBox();
        _opacitySlider.MinValue = 0;
        _opacitySlider.MaxValue = 100;
        _opacitySlider.Step = 1;
        _opacitySlider.Value = 100;
        _opacitySlider.CustomMinimumSize = new Vector2(60, 0);
        _opacitySlider.ValueChanged += OnOpacityChanged;
        _toolbar.AddChild(_opacitySlider);
        
        // Botones de acción
        var gridBtn = new Button();
        gridBtn.Text = "Grid (G)";
        gridBtn.Pressed += () => _main.ShowGrid = !_main.ShowGrid;
        _toolbar.AddChild(gridBtn);
        
        var zoomInBtn = new Button();
        zoomInBtn.Text = "+ Zoom";
        zoomInBtn.Pressed += () => { _main.Zoom = Mathf.Min(_main.Zoom * 1.2f, 10f); _main.UpdateCanvasTransform(); };
        _toolbar.AddChild(zoomInBtn);
        
        var zoomOutBtn = new Button();
        zoomOutBtn.Text = "- Zoom";
        zoomOutBtn.Pressed += () => { _main.Zoom = Mathf.Max(_main.Zoom / 1.2f, 0.1f); _main.UpdateCanvasTransform(); };
        _toolbar.AddChild(zoomOutBtn);
        
        var exportBtn = new Button();
        exportBtn.Text = "Exportar PNG";
        exportBtn.Pressed += _main.ExportImage;
        _toolbar.AddChild(exportBtn);
        
        // Spacer para empujar el panel de capas a la derecha
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 0);
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _toolbar.AddChild(spacer);
        
        // === ÁREA CENTRAL (dividida en canvas + panel lateral) ===
        var centerContainer = new HBoxContainer();
        centerContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(centerContainer);
        
        // El canvas ya está creado en Main.cs, lo movemos aquí si es necesario
        // Por ahora dejamos que Main maneje su propio container
        
        // === PANEL LATERAL DE CAPAS ===
        _layersPanel = new VBoxContainer();
        _layersPanel.CustomMinimumSize = new Vector2(200, 0);
        centerContainer.AddChild(_layersPanel);
        
        var layersTitle = new Label();
        layersTitle.Text = "Capas";
        layersTitle.AddThemeOverride("font_size", 14);
        _layersPanel.AddChild(layersTitle);
        
        // Lista de capas
        _layerList = new ItemList();
        _layerList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _layerList.ItemSelected += OnLayerSelected;
        _layersPanel.AddChild(_layerList);
        
        // Botones de capas
        var layerButtons = new HBoxContainer();
        
        _newLayerBtn = new Button();
        _newLayerBtn.Text = "+";
        _newLayerBtn.TooltipText = "Nueva capa";
        _newLayerBtn.Pressed += OnNewLayer;
        layerButtons.AddChild(_newLayerBtn);
        
        _deleteLayerBtn = new Button();
        _deleteLayerBtn.Text = "-";
        _deleteLayerBtn.TooltipText = "Eliminar capa";
        _deleteLayerBtn.Pressed += OnDeleteLayer;
        layerButtons.AddChild(_deleteLayerBtn);
        
        _duplicateLayerBtn = new Button();
        _duplicateLayerBtn.Text = "Dup";
        _duplicateLayerBtn.TooltipText = "Duplicar capa";
        _duplicateLayerBtn.Pressed += OnDuplicateLayer;
        layerButtons.AddChild(_duplicateLayerBtn);
        
        _mergeDownBtn = new Button();
        _mergeDownBtn.Text = "Merge";
        _mergeDownBtn.TooltipText = "Fusionar abajo";
        _mergeDownBtn.Pressed += OnMergeDown;
        layerButtons.AddChild(_mergeDownBtn);
        
        _flattenBtn = new Button();
        _flattenBtn.Text = "Flat";
        _flattenBtn.TooltipText = "Aplanar todas";
        _flattenBtn.Pressed += OnFlatten;
        layerButtons.AddChild(_flattenBtn);
        
        _layersPanel.AddChild(layerButtons);
        
        // === BARRA DE ESTADO INFERIOR ===
        var statusBar = new HBoxContainer();
        mainContainer.AddChild(statusBar);
        
        _statusLabel = new Label();
        _statusLabel.Text = "PhotoGodot Pro v2.0 - Listo";
        statusBar.AddChild(_statusLabel);
        
        var coordsLabel = new Label();
        coordsLabel.Name = "Coords";
        coordsLabel.Text = "(0, 0)";
        coordsLabel.AddThemeOverride("font_size", 11);
        statusBar.AddChild(coordsLabel);
    }

    private void ConnectSignals()
    {
        if (_main.LayerManager != null)
        {
            _main.LayerManager.LayerListChanged += RefreshLayerPanel;
            _main.LayerManager.ActiveLayerChanged += OnActiveLayerChanged;
        }
        
        if (_main.ToolManager != null)
        {
            _main.ToolManager.ToolChanged += OnToolChanged;
        }
    }

    private void OnToolSelected(int index)
    {
        string[] tools = { "Brush", "Eraser", "ColorPicker", "Move", "Select" };
        if (index >= 0 && index < tools.Length)
        {
            _main.ToolManager.SetTool(tools[index]);
        }
    }

    private void OnColorChanged(Color color)
    {
        _main.CurrentColor = color;
    }

    private void OnBrushSizeChanged(double value)
    {
        _main.BrushSize = (float)value;
    }

    private void OnOpacityChanged(double value)
    {
        _main.BrushOpacity = (float)(value / 100.0);
    }

    public void RefreshLayerPanel()
    {
        _layerList.Clear();
        
        if (_main.LayerManager == null) return;
        
        int activeIndex = -1;
        for (int i = 0; i < _main.LayerManager.Layers.Count; i++)
        {
            var layer = _main.LayerManager.Layers[i];
            string iconType = layer.IsVisible ? "👁" : "❌";
            string displayName = $"{iconType} {layer.LayerName}";
            
            _layerList.AddItem(displayName);
            
            if (layer == _main.LayerManager.ActiveLayer)
            {
                activeIndex = i;
            }
        }
        
        if (activeIndex >= 0)
        {
            _layerList.Select(activeIndex);
        }
    }

    private void OnLayerSelected(int index)
    {
        if (_main.LayerManager != null && index >= 0 && index < _main.LayerManager.Layers.Count)
        {
            var layer = _main.LayerManager.Layers[index];
            _main.LayerManager.SetActiveLayer(layer);
        }
    }

    private void OnActiveLayerChanged(Layer layer)
    {
        RefreshLayerPanel();
        _statusLabel.Text = $"Capa activa: {layer.LayerName}";
    }

    private void OnToolChanged(BaseTool tool)
    {
        _statusLabel.Text = $"Herramienta: {tool.ToolName}";
        
        // Actualizar selector
        string[] tools = { "Brush", "Eraser", "ColorPicker", "Move", "Select" };
        int index = Array.IndexOf(tools, tool.ToolName);
        if (index >= 0)
        {
            _toolSelector.Selected = index;
        }
    }

    private void OnNewLayer()
    {
        _main.LayerManager.CreateLayer($"Capa {_main.LayerManager.LayerCount + 1}");
    }

    private void OnDeleteLayer()
    {
        if (_main.LayerManager.ActiveLayer != null)
        {
            _main.LayerManager.RemoveLayer(_main.LayerManager.ActiveLayer);
        }
    }

    private void OnDuplicateLayer()
    {
        if (_main.LayerManager.ActiveLayer != null)
        {
            _main.LayerManager.DuplicateLayer(_main.LayerManager.ActiveLayer);
        }
    }

    private void OnMergeDown()
    {
        _main.LayerManager.MergeDown();
    }

    private void OnFlatten()
    {
        _main.LayerManager.Flatten();
    }
}
