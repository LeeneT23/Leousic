using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.UI;

public partial class MainUI : CanvasLayer
{
    private Main _main;
    private LayerManager _layerManager;
    private ToolManager _toolManager;
    private HistoryManager _historyManager;
    
    // Contenedores UI
    private Control _toolbar;
    private Control _layersPanel;
    private Control _propertiesPanel;
    private ColorPickerButton _colorPicker;
    private Label _zoomLabel;
    private VBoxCompat _layersList;
    private Button _newLayerBtn;
    private Button _deleteLayerBtn;
    private Button _mergeDownBtn;
    private Button _flattenBtn;

    public override void _Ready()
    {
        Layer = 10; // UI por encima de todo
    }

    public void Setup(Main main, LayerManager lm, ToolManager tm, HistoryManager hm)
    {
        _main = main;
        _layerManager = lm;
        _toolManager = tm;
        _historyManager = hm;
        
        BuildUI();
        ConnectSignals();
    }

    private void BuildUI()
    {
        // Toolbar superior
        CreateToolbar();
        
        // Panel de propiedades (izquierda)
        CreatePropertiesPanel();
        
        // Panel de capas (derecha)
        CreateLayersPanel();
        
        // Barra de estado (inferior)
        CreateStatusBar();
    }

    private void CreateToolbar()
    {
        _toolbar = new Control();
        _toolbar.Name = "Toolbar";
        _toolbar.AnchorRight = 1.0f;
        _toolbar.OffsetBottom = 50;
        _toolbar.BackgroundColor = new Color(0.2f, 0.2f, 0.2f);
        AddChild(_toolbar);
        
        // Botones de herramientas
        string[] tools = { "Pincel", "Borrador", "Selector", "Mover", "Seleccionar" };
        string[] icons = { "🖌️", "🧹", "💉", "✋", "⬚" };
        
        HBoxCompat hbox = new HBoxCompat();
        hbox.AddThemeConstantOverride("separation", 5);
        hbox.SetPosition(new Vector2(10, 5));
        _toolbar.AddChild(hbox);
        
        for (int i = 0; i < tools.Length; i++)
        {
            Button btn = new Button();
            btn.Text = $"{icons[i]} {tools[i]}";
            btn.CustomMinimumSize = new Vector2(100, 40);
            btn.Pressed += () => _toolManager.SetTool(tools[i]);
            hbox.AddChild(btn);
        }
        
        // Selector de color
        _colorPicker = new ColorPickerButton();
        _colorPicker.Color = Colors.Black;
        _colorPicker.CustomMinimumSize = new Vector2(60, 40);
        _colorPicker.Position = new Vector2(600, 5);
        _colorPicker.ColorChanged += (color) => _main.SetCurrentColor(color);
        _toolbar.AddChild(_colorPicker);
    }

    private void CreatePropertiesPanel()
    {
        _propertiesPanel = new Control();
        _propertiesPanel.Name = "PropertiesPanel";
        _propertiesPanel.OffsetRight = 250;
        _propertiesPanel.AnchorBottom = 1.0f;
        _propertiesPanel.OffsetBottom = -50;
        _propertiesPanel.BackgroundColor = new Color(0.15f, 0.15f, 0.15f);
        AddChild(_propertiesPanel);
        
        VBoxCompat vbox = new VBoxCompat();
        vbox.AddThemeConstantOverride("separation", 10);
        vbox.SetPosition(new Vector2(10, 10));
        vbox.SetSize(new Vector2(230, 400));
        _propertiesPanel.AddChild(vbox);
        
        // Título
        Label title = new Label();
        title.Text = "Propiedades";
        title.AddThemeColorOverride("font_color", Colors.White);
        vbox.AddChild(title);
        
        // Tamaño de pincel
        Label brushLabel = new Label();
        brushLabel.Text = "Tamaño Pincel:";
        brushLabel.AddThemeColorOverride("font_color", Colors.White);
        vbox.AddChild(brushLabel);
        
        HSlider brushSlider = new HSlider();
        brushSlider.MinValue = 1;
        brushSlider.MaxValue = 100;
        brushSlider.Step = 1;
        brushSlider.Value = 10;
        brushSlider.ValueChanged += (value) => {
            var brush = _toolManager.GetToolByName("Pincel") as Tools.BrushTool;
            if (brush != null) brush.BrushSize = (float)value;
        };
        vbox.AddChild(brushSlider);
        
        // Opacidad
        Label opacityLabel = new Label();
        opacityLabel.Text = "Opacidad:";
        opacityLabel.AddThemeColorOverride("font_color", Colors.White);
        vbox.AddChild(opacityLabel);
        
        HSlider opacitySlider = new HSlider();
        opacitySlider.MinValue = 0;
        opacitySlider.MaxValue = 1;
        opacitySlider.Step = 0.01f;
        opacitySlider.Value = 1;
        opacitySlider.ValueChanged += (value) => {
            var brush = _toolManager.GetToolByName("Pincel") as Tools.BrushTool;
            if (brush != null) brush.Opacity = (float)value;
            var eraser = _toolManager.GetToolByName("Borrador") as Tools.EraserTool;
            if (eraser != null) eraser.Opacity = (float)value;
        };
        vbox.AddChild(opacitySlider);
    }

    private void CreateLayersPanel()
    {
        _layersPanel = new Control();
        _layersPanel.Name = "LayersPanel";
        _layersPanel.AnchorRight = 1.0f;
        _layersPanel.OffsetLeft = -250;
        _layersPanel.AnchorBottom = 1.0f;
        _layersPanel.OffsetBottom = -50;
        _layersPanel.BackgroundColor = new Color(0.15f, 0.15f, 0.15f);
        AddChild(_layersPanel);
        
        VBoxCompat vbox = new VBoxCompat();
        vbox.AddThemeConstantOverride("separation", 5);
        vbox.SetPosition(new Vector2(10, 10));
        vbox.SetSize(new Vector2(230, 400));
        _layersPanel.AddChild(vbox);
        
        // Título
        Label title = new Label();
        title.Text = "Capas";
        title.AddThemeColorOverride("font_color", Colors.White);
        vbox.AddChild(title);
        
        // Lista de capas
        ScrollContainer scroll = new ScrollContainer();
        scroll.CustomMinimumSize = new Vector2(230, 300);
        vbox.AddChild(scroll);
        
        _layersList = new VBoxCompat();
        _layersList.AddThemeConstantOverride("separation", 2);
        scroll.AddChild(_layersList);
        
        // Botones de capas
        HBoxCompat btnHbox = new HBoxCompat();
        btnHbox.AddThemeConstantOverride("separation", 5);
        vbox.AddChild(btnHbox);
        
        _newLayerBtn = new Button();
        _newLayerBtn.Text = "+ Nueva";
        _newLayerBtn.Pressed += () => _layerManager.CreateLayer($"Capa {_layerManager.LayerCount + 1}");
        btnHbox.AddChild(_newLayerBtn);
        
        _deleteLayerBtn = new Button();
        _deleteLayerBtn.Text = "Eliminar";
        _deleteLayerBtn.Pressed += () => {
            if (_layerManager.ActiveLayer != null)
                _layerManager.RemoveLayer(_layerManager.ActiveLayer);
        };
        btnHbox.AddChild(_deleteLayerBtn);
        
        _mergeDownBtn = new Button();
        _mergeDownBtn.Text = "Fusionar";
        _mergeDownBtn.Pressed += () => _layerManager.MergeDown();
        btnHbox.AddChild(_mergeDownBtn);
        
        _flattenBtn = new Button();
        _flattenBtn.Text = "Aplanar";
        _flattenBtn.Pressed += () => _layerManager.Flatten();
        btnHbox.AddChild(_flattenBtn);
        
        RefreshLayersList();
    }

    private void CreateStatusBar()
    {
        Control statusBar = new Control();
        statusBar.Name = "StatusBar";
        statusBar.AnchorTop = 1.0f;
        statusBar.AnchorRight = 1.0f;
        statusBar.OffsetTop = -30;
        statusBar.OffsetBottom = 0;
        statusBar.BackgroundColor = new Color(0.1f, 0.1f, 0.1f);
        AddChild(statusBar);
        
        HBoxCompat hbox = new HBoxCompat();
        hbox.AddThemeConstantOverride("separation", 20);
        hbox.SetPosition(new Vector2(10, 5));
        statusBar.AddChild(hbox);
        
        Label infoLabel = new Label();
        infoLabel.Text = "PhotoGodot Pro v2.0 | B: Pincel, E: Borrador, I: Selector, V: Mover, M: Selección, G: Grid, Ctrl+Z: Undo, Ctrl+S: Exportar";
        infoLabel.AddThemeColorOverride("font_color", Colors.Gray);
        hbox.AddChild(infoLabel);
        
        _zoomLabel = new Label();
        _zoomLabel.Text = "Zoom: 100%";
        _zoomLabel.AddThemeColorOverride("font_color", Colors.Gray);
        hbox.AddChild(_zoomLabel);
    }

    private void ConnectSignals()
    {
        _layerManager.LayerListChanged += RefreshLayersList;
        _layerManager.ActiveLayerChanged += (layer) => RefreshLayersList();
    }

    private void RefreshLayersList()
    {
        foreach (var child in _layersList.GetChildren())
        {
            child.QueueFree();
        }
        
        int idx = 0;
        foreach (var layer in _layerManager.Layers)
        {
            Button layerBtn = new Button();
            layerBtn.Text = $"{(layer == _layerManager.ActiveLayer ? "▶ " : "")}{layer.LayerName}";
            layerBtn.AddThemeColorOverride("font_color", layer == _layerManager.ActiveLayer ? Colors.Yellow : Colors.White);
            
            int captureIdx = idx;
            layerBtn.Pressed += () => _layerManager.SetActiveLayer(layer);
            
            // Doble click para renombrar (simplificado)
            _layersList.AddChild(layerBtn);
            idx++;
        }
    }

    public void UpdateColorPicker(Color color)
    {
        if (_colorPicker != null)
            _colorPicker.Color = color;
    }

    public void UpdateZoomLabel(float zoom)
    {
        if (_zoomLabel != null)
            _zoomLabel.Text = $"Zoom: {(int)(zoom * 100)}%";
    }
}

// Clases compat para Godot 4.x
public partial class VBoxCompat : VBoxContainer { }
public partial class HBoxCompat : HBoxContainer { }
