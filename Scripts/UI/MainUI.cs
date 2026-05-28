using Godot;
using System.Collections.Generic;

public partial class MainUI : Control
{
    private Main _main;
    
    // Menu buttons
    private MenuButton _menuFile, _menuEdit, _menuLayer, _menuView;
    
    // Tool buttons
    private Button _btnBrush, _btnEraser, _btnPicker, _btnMove, _btnSelect;
    private Label _toolLabel;
    
    // Tool options
    private ColorPickerButton _colorPicker;
    private ColorRect _colorPreview;
    private HSlider _brushSizeSlider, _opacitySlider, _hardnessSlider;
    
    // Layer controls
    private VBoxContainer _layersList;
    private Button _btnNewLayer, _btnDeleteLayer;
    
    // Filter buttons
    private Button _btnGrayscale, _btnInvert, _btnBlur, _btnSharpen;
    
    // Status bar
    private Label _statusLabel, _zoomLabel;
    
    public void Initialize(Main main)
    {
        _main = main;
        SetupMenus();
        ConnectSignals();
        UpdateToolLabel("Brush");
        GD.Print("UI Initialized");
    }
    
    private void SetupMenus()
    {
        // File menu
        _menuFile = GetNode<MenuButton>("MarginContainer/VBoxContainer/TopPanel/MenuBar/MenuButton_File");
        var filePopup = _menuFile.GetPopup();
        filePopup.IdPressed += OnFileMenuSelected;
        
        filePopup.AddItem("New", 0, Key.CtrlMask | Key.N);
        filePopup.AddItem("Open...", 1, Key.CtrlMask | Key.O);
        filePopup.AddItem("Save", 2, Key.CtrlMask | Key.S);
        filePopup.AddItem("Export As PNG...", 3, Key.CtrlMask | Key.E);
        filePopup.AddSeparator();
        filePopup.AddItem("Exit", 4, Key.CtrlMask | Key.Q);
        
        // Edit menu
        _menuEdit = GetNode<MenuButton>("MarginContainer/VBoxContainer/TopPanel/MenuBar/MenuButton_Edit");
        var editPopup = _menuEdit.GetPopup();
        editPopup.IdPressed += OnEditMenuSelected;
        
        editPopup.AddItem("Undo", 0, Key.CtrlMask | Key.Z);
        editPopup.AddItem("Redo", 1, Key.CtrlMask | Key.Y);
        editPopup.AddSeparator();
        editPopup.AddItem("Cut", 2, Key.CtrlMask | Key.X);
        editPopup.AddItem("Copy", 3, Key.CtrlMask | Key.C);
        editPopup.AddItem("Paste", 4, Key.CtrlMask | Key.V);
        editPopup.AddSeparator();
        editPopup.AddItem("Clear", 5, Key.Delete);
        
        // Layer menu
        _menuLayer = GetNode<MenuButton>("MarginContainer/VBoxContainer/TopPanel/MenuBar/MenuButton_Layer");
        var layerPopup = _menuLayer.GetPopup();
        layerPopup.IdPressed += OnLayerMenuSelected;
        
        layerPopup.AddItem("New Layer", 0, Key.CtrlMask | Key.ShiftMask | Key.N);
        layerPopup.AddItem("Duplicate Layer", 1, Key.CtrlMask | Key.J);
        layerPopup.AddItem("Delete Layer", 2, Key.CtrlMask | Key.ShiftMask | Key.D);
        layerPopup.AddSeparator();
        layerPopup.AddItem("Merge Down", 3, Key.CtrlMask | Key.E);
        layerPopup.AddItem("Flatten Image", 4, Key.CtrlMask | Key.ShiftMask | Key.E);
        
        // View menu
        _menuView = GetNode<MenuButton>("MarginContainer/VBoxContainer/TopPanel/MenuBar/MenuButton_View");
        var viewPopup = _menuView.GetPopup();
        viewPopup.IdPressed += OnViewMenuSelected;
        
        viewPopup.AddItem("Toggle Grid", 0, Key.G);
        viewPopup.AddItem("Zoom In", 1, Key.CtrlMask | Key.Plus);
        viewPopup.AddItem("Zoom Out", 2, Key.CtrlMask | Key.Minus);
        viewPopup.AddItem("Reset Zoom", 3, Key.CtrlMask | Key.Key0);
        
        // Tool buttons
        _btnBrush = GetNode<Button>("MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_Brush");
        _btnEraser = GetNode<Button>("MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_Eraser");
        _btnPicker = GetNode<Button>("MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_Picker");
        _btnMove = GetNode<Button>("MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_Move");
        _btnSelect = GetNode<Button>("MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_Select");
        _toolLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopPanel/ToolBar/CurrentToolLabel");
        
        // Tool options
        _colorPicker = GetNode<ColorPickerButton>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/ColorPicker");
        _colorPreview = GetNode<ColorRect>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/PrimaryColorPreview");
        _brushSizeSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/BrushSizeSlider");
        _opacitySlider = GetNode<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/OpacitySlider");
        _hardnessSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/TopPanel/ToolOptions/HardnessSlider");
        
        // Layer controls
        _layersList = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/LayersPanel/VBoxContainer/LayersList");
        _btnNewLayer = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/LayersPanel/VBoxContainer/LayersButtons/NewLayerButton");
        _btnDeleteLayer = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/LayersPanel/VBoxContainer/LayersButtons/DeleteLayerButton");
        
        // Filter buttons
        _btnGrayscale = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/FiltersPanel/VBoxContainer/GrayscaleButton");
        _btnInvert = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/FiltersPanel/VBoxContainer/InvertButton");
        _btnBlur = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/FiltersPanel/VBoxContainer/BlurButton");
        _btnSharpen = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/RightPanel/FiltersPanel/VBoxContainer/SharpenButton");
        
        // Status bar
        _statusLabel = GetNode<Label>("MarginContainer/VBoxContainer/StatusBar/StatusInfo");
        _zoomLabel = GetNode<Label>("MarginContainer/VBoxContainer/StatusBar/ZoomLabel");
    }
    
    private void ConnectSignals()
    {
        _colorPicker.ColorChanged += OnColorChanged;
        _brushSizeSlider.ValueChanged += OnBrushSizeChanged;
        _opacitySlider.ValueChanged += OnOpacityChanged;
        _hardnessSlider.ValueChanged += OnHardnessChanged;
        
        _btnNewLayer.Pressed += OnNewLayer;
        _btnDeleteLayer.Pressed += OnDeleteLayer;
        
        _btnGrayscale.Pressed += OnApplyGrayscale;
        _btnInvert.Pressed += OnApplyInvert;
        _btnBlur.Pressed += OnApplyBlur;
        _btnSharpen.Pressed += OnApplySharpen;
    }
    
    #region Menu Handlers
    
    private void OnFileMenuSelected(int id)
    {
        switch (id)
        {
            case 0: _main.CreateNewDocument(); break;
            case 1: _main.OpenProject(); break;
            case 2: _main.SaveProject(); break;
            case 3: ExportAsPNG(); break;
            case 4: GetTree().Quit(); break;
        }
    }
    
    private void OnEditMenuSelected(int id)
    {
        switch (id)
        {
            case 0: _main.GetHistoryManager().Undo(); break;
            case 1: _main.GetHistoryManager().Redo(); break;
        }
    }
    
    private void OnLayerMenuSelected(int id)
    {
        var layerManager = _main.GetLayerManager();
        switch (id)
        {
            case 0: layerManager.CreateLayer(); break;
            case 1: layerManager.DuplicateLayer(layerManager.ActiveLayerIndex); break;
            case 2: layerManager.DeleteLayer(layerManager.ActiveLayerIndex); break;
            case 3: layerManager.MergeDown(layerManager.ActiveLayerIndex); break;
        }
    }
    
    private void OnViewMenuSelected(int id)
    {
        switch (id)
        {
            case 0: _main.ToggleGrid(); break;
        }
    }
    
    #endregion
    
    #region Tool Handlers
    
    public void OnSelectBrush() => _main.GetToolManager().SetActiveTool("Brush");
    public void OnSelectEraser() => _main.GetToolManager().SetActiveTool("Eraser");
    public void OnSelectColorPicker() => _main.GetToolManager().SetActiveTool("ColorPicker");
    public void OnSelectMove() => _main.GetToolManager().SetActiveTool("Move");
    public void OnSelectSelect() => _main.GetToolManager().SetActiveTool("Select");
    
    #endregion
    
    #region Layer Handlers
    
    public void OnNewLayer()
    {
        _main.GetLayerManager().CreateLayer($"Layer {_main.GetLayerManager().LayerCount + 1}");
    }
    
    public void OnDeleteLayer()
    {
        _main.GetLayerManager().DeleteLayer(_main.GetLayerManager().ActiveLayerIndex);
    }
    
    #endregion
    
    #region Filter Handlers
    
    public void OnApplyGrayscale()
    {
        _main.GetLayerManager().ApplyFilterToActiveLayer(color =>
        {
            float gray = color.R * 0.299f + color.G * 0.587f + color.B * 0.114f;
            return new Color(gray, gray, gray, color.A);
        });
        SaveHistoryState();
    }
    
    public void OnApplyInvert()
    {
        _main.GetLayerManager().ApplyFilterToActiveLayer(color =>
        {
            return new Color(1 - color.R, 1 - color.G, 1 - color.B, color.A);
        });
        SaveHistoryState();
    }
    
    public void OnApplyBlur()
    {
        GD.Print("Blur filter - simplified implementation");
        // Simplified blur - would need proper convolution for production
        OnApplyGrayscale(); // Placeholder
    }
    
    public void OnApplySharpen()
    {
        GD.Print("Sharpen filter - placeholder");
        // Placeholder for sharpen effect
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnColorChanged(Color color)
    {
        _main.SetPrimaryColor(color);
        if (_colorPreview != null)
        {
            _colorPreview.Color = color;
        }
    }
    
    private void OnBrushSizeChanged(float value)
    {
        _main.SetBrushSize(value);
    }
    
    private void OnOpacityChanged(float value)
    {
        _main.SetOpacity(value / 100.0f);
    }
    
    private void OnHardnessChanged(float value)
    {
        _main.SetHardness(value / 100.0f);
    }
    
    #endregion
    
    #region UI Updates
    
    public void UpdateToolLabel(string toolName)
    {
        if (_toolLabel != null)
        {
            _toolLabel.Text = toolName;
        }
        
        // Highlight active tool button
        ResetToolButtons();
        string btnName = toolName.ToLower();
        var btn = GetNodeOrNull<Button>($"MarginContainer/VBoxContainer/TopPanel/ToolBar/ToolButton_{btnName.Capitalize()}");
        if (btn != null)
        {
            btn.ButtonPressed = true;
        }
    }
    
    private void ResetToolButtons()
    {
        if (_btnBrush != null) _btnBrush.ButtonPressed = false;
        if (_btnEraser != null) _btnEraser.ButtonPressed = false;
        if (_btnPicker != null) _btnPicker.ButtonPressed = false;
        if (_btnMove != null) _btnMove.ButtonPressed = false;
        if (_btnSelect != null) _btnSelect.ButtonPressed = false;
    }
    
    public void UpdateColorPreview(Color color)
    {
        if (_colorPreview != null)
        {
            _colorPreview.Color = color;
        }
    }
    
    public void UpdateColorPicker(Color color)
    {
        if (_colorPicker != null)
        {
            _colorPicker.Color = color;
        }
        UpdateColorPreview(color);
    }
    
    public void UpdateLayersList()
    {
        if (_layersList == null) return;
        
        // Clear existing layer items
        foreach (var child in _layersList.GetChildren())
        {
            child.QueueFree();
        }
        
        var layerManager = _main.GetLayerManager();
        for (int i = layerManager.LayerCount - 1; i >= 0; i--)
        {
            var layer = layerManager.GetNode<Layer>($"Layer_{i}");
            var hBox = new HBoxContainer();
            
            var visibilityBtn = new CheckBox();
            visibilityBtn.Toggled += pressed => ToggleLayerVisibility(i);
            visibilityBtn.ButtonPressed = layer.Visible;
            hBox.AddChild(visibilityBtn);
            
            var nameLabel = new Label();
            nameLabel.Text = $"{layer.Name}";
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hBox.AddChild(nameLabel);
            
            var selectBtn = new Button();
            selectBtn.Text = "Select";
            selectBtn.Pressed += () => SelectLayer(i);
            hBox.AddChild(selectBtn);
            
            if (i == layerManager.ActiveLayerIndex)
            {
                var highlight = new ColorRect();
                highlight.Color = new Color(0.2f, 0.4f, 0.6f, 0.3f);
                highlight.SetAnchorsPreset(Control.LayoutPreset.FullRect);
                hBox.AddChild(highlight);
            }
            
            _layersList.AddChild(hBox);
        }
    }
    
    private void ToggleLayerVisibility(int index)
    {
        // Implementation would toggle layer visibility
    }
    
    private void SelectLayer(int index)
    {
        _main.GetLayerManager().SetActiveLayer(index);
    }
    
    public void UpdateStatus(string status)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = status;
        }
    }
    
    public void UpdateZoomLabel(float zoom)
    {
        if (_zoomLabel != null)
        {
            _zoomLabel.Text = $"{(int)(zoom * 100)}%";
        }
    }
    
    #endregion
    
    private void ExportAsPNG()
    {
        var dialog = new FileDialog
        {
            Title = "Export as PNG",
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.png ; PNG Image" }
        };
        
        AddChild(dialog);
        dialog.PopupCentered();
        dialog.FileSelected += path =>
        {
            _main.GetLayerManager().ExportToPNG(path);
            UpdateStatus($"Exported: {path}");
        };
        dialog.CloseRequested += () => dialog.QueueFree();
    }
    
    private void SaveHistoryState()
    {
        var compositedImage = _main.GetLayerManager().GetCompositedImage();
        if (compositedImage != null)
        {
            _main.GetHistoryManager().SaveState(compositedImage);
        }
    }
}
