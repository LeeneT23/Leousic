using Godot;
using PhotoGodot.Core;
using System;

namespace PhotoGodot.UI;

public partial class MainUI : CanvasLayer
{
    private Main _main;
    
    // Toolbar
    private VBoxContainer _toolbar;
    private Button _brushBtn, _eraserBtn, _pickerBtn, _moveBtn, _selectBtn;
    
    // Color Pickers
    private ColorPickerButton _primaryColorPicker;
    private ColorPickerButton _secondaryColorPicker;
    
    // Brush Settings
    private VBoxContainer _brushSettings;
    private HSlider _sizeSlider;
    private HSlider _hardnessSlider;
    private HSlider _opacitySlider;
    
    // Layers Panel
    private VBoxContainer _layersPanel;
    private ItemList _layerList;
    private Button _newLayerBtn, _deleteLayerBtn, _duplicateLayerBtn, _mergeDownBtn, _flattenBtn;
    
    // Status Bar
    private Label _statusLabel;
    private Label _zoomLabel;
    private Label _cursorLabel;
    
    // Menu Bar
    private MenuBar _menuBar;
    
    // Notification
    private Panel _notificationPanel;
    private Label _notificationLabel;
    private Timer _notificationTimer;

    public override void _Ready()
    {
        CreateUI();
    }

    public void Initialize(Main main)
    {
        _main = main;
        
        // Conectar señales
        _main.LayerManager.LayerListChanged += UpdateLayerList;
        _main.LayerManager.ActiveLayerChanged += UpdateActiveLayer;
        _main.ToolManager.ToolChanged += OnToolChanged;
    }

    private void CreateUI()
    {
        // Menu Bar superior
        CreateMenuBar();
        
        // Toolbar izquierda
        CreateToolbar();
        
        // Panel derecho (capas y propiedades)
        CreateRightPanel();
        
        // Barra de estado inferior
        CreateStatusBar();
        
        // Notificación
        CreateNotification();
    }

    private void CreateMenuBar()
    {
        var menuContainer = new HBoxContainer();
        menuContainer.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        menuContainer.Position = new Vector2(0, 0);
        menuContainer.Size = new Vector2(1280, 30);
        AddChild(menuContainer);

        // Botones de menú
        var fileMenu = CreateMenuButton("Archivo", new[] { "Nuevo (Ctrl+N)", "Exportar (Ctrl+S)" });
        fileMenu.IdPressed += id => {
            if (id == 0) _main.NewFile();
            if (id == 1) _main.ExportImage();
        };
        menuContainer.AddChild(fileMenu);

        var editMenu = CreateMenuButton("Editar", new[] { "Deshacer (Ctrl+Z)", "Rehacer (Ctrl+Shift+Z)" });
        editMenu.IdPressed += id => {
            if (id == 0) _main.HistoryManager.Undo();
            if (id == 1) _main.HistoryManager.Redo();
        };
        menuContainer.AddChild(editMenu);

        var viewMenu = CreateMenuButton("Ver", new[] { "Zoom In", "Zoom Out", "Grid (G)" });
        viewMenu.IdPressed += id => {
            if (id == 0) _main.SetZoom(_main.Zoom + 0.1f);
            if (id == 1) _main.SetZoom(_main.Zoom - 0.1f);
            if (id == 2) _main.ToggleGrid();
        };
        menuContainer.AddChild(viewMenu);
    }

    private MenuButton CreateMenuButton(string text, string[] items)
    {
        var menu = new MenuButton();
        menu.Text = text;
        var popup = menu.GetPopup();
        foreach (var item in items)
        {
            popup.AddItem(item);
        }
        return menu;
    }

    private void CreateToolbar()
    {
        _toolbar = new VBoxContainer();
        _toolbar.SetAnchorsPreset(Control.LayoutPreset.LeftWide);
        _toolbar.Position = new Vector2(0, 30);
        _toolbar.Size = new Vector2(50, 690);
        AddChild(_toolbar);

        _brushBtn = CreateToolButton("🖌️", "Pincel (B)");
        _brushBtn.Pressed += () => _main.ToolManager.SetTool("Pincel");
        _toolbar.AddChild(_brushBtn);

        _eraserBtn = CreateToolButton("🧼", "Borrador (E)");
        _eraserBtn.Pressed += () => _main.ToolManager.SetTool("Borrador");
        _toolbar.AddChild(_eraserBtn);

        _pickerBtn = CreateToolButton("💉", "Selector (I)");
        _pickerBtn.Pressed += () => _main.ToolManager.SetTool("Selector");
        _toolbar.AddChild(_pickerBtn);

        _moveBtn = CreateToolButton("✋", "Mover (V)");
        _moveBtn.Pressed += () => _main.ToolManager.SetTool("Mover");
        _toolbar.AddChild(_moveBtn);

        _selectBtn = CreateToolButton("⬜", "Selección (M)");
        _selectBtn.Pressed += () => _main.ToolManager.SetTool("Selección");
        _toolbar.AddChild(_selectBtn);

        _toolbar.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        // Selector de color primario
        _primaryColorPicker = new ColorPickerButton();
        _primaryColorPicker.Color = Colors.Black;
        _primaryColorPicker.CustomMinimumSize = new Vector2(40, 40);
        _primaryColorPicker.ColorChanged += color => _main.PrimaryColor = color;
        _toolbar.AddChild(_primaryColorPicker);

        // Selector de color secundario
        _secondaryColorPicker = new ColorPickerButton();
        _secondaryColorPicker.Color = Colors.White;
        _secondaryColorPicker.CustomMinimumSize = new Vector2(40, 40);
        _secondaryColorPicker.ColorChanged += color => _main.SecondaryColor = color;
        _toolbar.AddChild(_secondaryColorPicker);
    }

    private Button CreateToolButton(string icon, string tooltip)
    {
        var btn = new Button();
        btn.Text = icon;
        btn.CustomMinimumSize = new Vector2(40, 40);
        btn.TooltipText = tooltip;
        btn.Flat = true;
        return btn;
    }

    private void CreateRightPanel()
    {
        var rightPanel = new VBoxContainer();
        rightPanel.SetAnchorsPreset(Control.LayoutPreset.RightWide);
        rightPanel.Position = new Vector2(-250, 30);
        rightPanel.Size = new Vector2(250, 690);
        AddChild(rightPanel);

        // Configuración del pincel
        _brushSettings = new VBoxContainer();
        rightPanel.AddChild(_brushSettings);

        _brushSettings.AddChild(CreateLabel("Tamaño del Pincel"));
        _sizeSlider = CreateSlider(1, 100, 10);
        _sizeSlider.ValueChanged += v => {
            var tool = _main.ToolManager.CurrentTool as Tools.BrushTool;
            if (tool != null) tool.BrushSize = (float)v;
        };
        _brushSettings.AddChild(_sizeSlider);

        _brushSettings.AddChild(CreateLabel("Dureza"));
        _hardnessSlider = CreateSlider(0, 1, 1);
        _hardnessSlider.ValueChanged += v => {
            var tool = _main.ToolManager.CurrentTool as Tools.BrushTool;
            if (tool != null) tool.BrushHardness = (float)v;
        };
        _brushSettings.AddChild(_hardnessSlider);

        _brushSettings.AddChild(CreateLabel("Opacidad"));
        _opacitySlider = CreateSlider(0, 1, 1);
        _opacitySlider.ValueChanged += v => {
            var tool = _main.ToolManager.CurrentTool as Tools.BrushTool;
            if (tool != null) tool.Opacity = (float)v;
        };
        _brushSettings.AddChild(_opacitySlider);

        _brushSettings.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        // Panel de capas
        _layersPanel = new VBoxContainer();
        rightPanel.AddChild(_layersPanel);

        _layersPanel.AddChild(CreateLabel("Capas"));

        var layerButtons = new HBoxContainer();
        _newLayerBtn = CreateSmallButton("+");
        _newLayerBtn.Pressed += () => _main.LayerManager.CreateLayer($"Capa {_main.LayerManager.LayerCount + 1}");
        layerButtons.AddChild(_newLayerBtn);

        _deleteLayerBtn = CreateSmallButton("-");
        _deleteLayerBtn.Pressed += () => {
            if (_main.LayerManager.ActiveLayer != null)
                _main.LayerManager.RemoveLayer(_main.LayerManager.ActiveLayer);
        };
        layerButtons.AddChild(_deleteLayerBtn);

        _duplicateLayerBtn = CreateSmallButton("📋");
        _duplicateLayerBtn.Pressed += () => _main.LayerManager.DuplicateLayer();
        layerButtons.AddChild(_duplicateLayerBtn);

        _mergeDownBtn = CreateSmallButton("⬇️");
        _mergeDownBtn.Pressed += () => _main.LayerManager.MergeDown();
        layerButtons.AddChild(_mergeDownBtn);

        _flattenBtn = CreateSmallButton("⚡");
        _flattenBtn.Pressed += () => _main.LayerManager.Flatten();
        layerButtons.AddChild(_flattenBtn);

        _layersPanel.AddChild(layerButtons);

        _layerList = new ItemList();
        _layerList.CustomMinimumSize = new Vector2(0, 200);
        _layerList.ItemSelected += index => {
            var layers = _main.LayerManager.Layers;
            if (index < layers.Count)
                _main.LayerManager.SetActiveLayer(layers[index]);
        };
        _layersPanel.AddChild(_layerList);

        UpdateLayerList();
    }

    private Label CreateLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        return label;
    }

    private HSlider CreateSlider(double min, double max, double value)
    {
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = 0.1;
        return slider;
    }

    private Button CreateSmallButton(string text)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(40, 30);
        return btn;
    }

    private void CreateStatusBar()
    {
        var statusBar = new HBoxContainer();
        statusBar.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        statusBar.Position = new Vector2(0, -30);
        statusBar.Size = new Vector2(1280, 30);
        AddChild(statusBar);

        _statusLabel = new Label();
        _statusLabel.Text = "PhotoGodot Pro v2.0 - Listo";
        statusBar.AddChild(_statusLabel);

        statusBar.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });

        _cursorLabel = new Label();
        _cursorLabel.Text = "X: 0 Y: 0";
        statusBar.AddChild(_cursorLabel);

        statusBar.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });

        _zoomLabel = new Label();
        _zoomLabel.Text = "100%";
        statusBar.AddChild(_zoomLabel);
    }

    private void CreateNotification()
    {
        _notificationPanel = new Panel();
        _notificationPanel.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
        _notificationPanel.Position = new Vector2(-150, 50);
        _notificationPanel.Size = new Vector2(300, 50);
        _notificationPanel.Visible = false;
        AddChild(_notificationPanel);

        _notificationLabel = new Label();
        _notificationLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _notificationLabel.VerticalAlignment = VerticalAlignment.Center;
        _notificationLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _notificationPanel.AddChild(_notificationLabel);

        _notificationTimer = new Timer();
        _notificationTimer.WaitTime = 2.0;
        _notificationTimer.OneShot = true;
        _notificationTimer.Timeout += () => _notificationPanel.Visible = false;
        AddChild(_notificationTimer);
    }

    public void UpdateLayerList()
    {
        _layerList.Clear();
        var layers = _main.LayerManager.Layers;
        
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            string icon = layer.IsVisible ? "👁️" : "🚫";
            string active = layer == _main.LayerManager.ActiveLayer ? " ✅" : "";
            _layerList.AddItem($"{icon} {layer.LayerName}{active}");
        }
    }

    public void UpdateActiveLayer(Layer layer)
    {
        UpdateLayerList();
    }

    public void UpdateCursorPosition(Vector2 pos)
    {
        _cursorLabel.Text = $"X: {(int)pos.X} Y: {(int)pos.Y}";
    }

    public void UpdateZoomDisplay(float zoom)
    {
        _zoomLabel.Text = $"{(int)(zoom * 100)}%";
    }

    public void ShowNotification(string message)
    {
        _notificationLabel.Text = message;
        _notificationPanel.Visible = true;
        _notificationTimer.Start();
    }

    private void OnToolChanged(BaseTool tool)
    {
        // Actualizar estado visual de los botones
        _brushBtn.Flat = tool.ToolName != "Pincel";
        _eraserBtn.Flat = tool.ToolName != "Borrador";
        _pickerBtn.Flat = tool.ToolName != "Selector";
        _moveBtn.Flat = tool.ToolName != "Mover";
        _selectBtn.Flat = tool.ToolName != "Selección";
        
        _statusLabel.Text = $"Herramienta: {tool.ToolName}";
    }
}
