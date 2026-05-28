using Godot;

namespace PhotoGodot.UI
{
    /// <summary>
    /// Main UI controller for PhotoGodot application
    /// </summary>
    public class MainUI : Control
    {
        // UI Components
        private VBoxContainer _toolbar;
        private HBoxContainer _toolOptions;
        private ColorPickerButton _colorPicker;
        private HSlider _brushSizeSlider;
        private HSlider _opacitySlider;
        private Label _statusLabel;
        private MenuBar _menuBar;
        
        // References
        private ToolManager _toolManager;
        private HistoryManager _historyManager;
        private DrawingCanvas _canvas;
        
        // Current colors
        private Color _primaryColor = Colors.Black;
        private Color _secondaryColor = Colors.White;
        
        public override void _Ready()
        {
            GD.Print("[MainUI] Initializing UI");
            
            // Get references to managers and canvas
            _toolManager = GetNodeOrNull<ToolManager>("../ToolManager");
            _historyManager = GetNodeOrNull<HistoryManager>("../HistoryManager");
            _canvas = GetNodeOrNull<DrawingCanvas>("../DrawingCanvas");
            
            SetupUI();
            ConnectSignals();
            UpdateToolOptions();
        }
        
        /// <summary>
        /// Setup all UI components
        /// </summary>
        private void SetupUI()
        {
            CreateMenuBar();
            CreateToolbar();
            CreateToolOptions();
            CreateStatusBar();
        }
        
        /// <summary>
        /// Create the menu bar
        /// </summary>
        private void CreateMenuBar()
        {
            _menuBar = new MenuBar();
            
            // File menu
            var fileMenu = new PopupMenu();
            fileMenu.Name = "File";
            fileMenu.IdPressed += OnMenuIdPressed;
            fileMenu.AddItem("New", 0, KeyMask.Ctrl | Key.N);
            fileMenu.AddItem("Open", 1, KeyMask.Ctrl | Key.O);
            fileMenu.AddItem("Save", 2, KeyMask.Ctrl | Key.S);
            fileMenu.AddItem("Export", 3, KeyMask.Ctrl | Key.E);
            fileMenu.AddSeparator();
            fileMenu.AddItem("Exit", 4, KeyMask.Alt | Key.F4);
            
            // Edit menu
            var editMenu = new PopupMenu();
            editMenu.Name = "Edit";
            editMenu.IdPressed += OnMenuIdPressed;
            editMenu.AddItem("Undo", 5, KeyMask.Ctrl | Key.Z);
            editMenu.AddItem("Redo", 6, KeyMask.Ctrl | Key.Y);
            editMenu.AddSeparator();
            editMenu.AddItem("Clear Canvas", 7);
            
            // View menu
            var viewMenu = new PopupMenu();
            viewMenu.Name = "View";
            viewMenu.IdPressed += OnMenuIdPressed;
            viewMenu.AddItem("Zoom In", 8, KeyMask.Ctrl | Key.Equal);
            viewMenu.AddItem("Zoom Out", 9, KeyMask.Ctrl | Key.Minus);
            viewMenu.AddItem("Reset Zoom", 10, KeyMask.Ctrl | Key.Key0);
            viewMenu.AddSeparator();
            viewMenu.AddItem("Toggle Grid", 11, KeyMask.Ctrl | Key.G);
            
            // Help menu
            var helpMenu = new PopupMenu();
            helpMenu.Name = "Help";
            helpMenu.IdPressed += OnMenuIdPressed;
            helpMenu.AddItem("About", 12);
            helpMenu.AddItem("Shortcuts", 13);
            
            _menuBar.AddChild(fileMenu);
            _menuBar.AddChild(editMenu);
            _menuBar.AddChild(viewMenu);
            _menuBar.AddChild(helpMenu);
            
            AddChild(_menuBar);
        }
        
        /// <summary>
        /// Create the toolbar with tool buttons
        /// </summary>
        private void CreateToolbar()
        {
            _toolbar = new VBoxContainer();
            _toolbar.Name = "Toolbar";
            
            // Tool button style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.2f, 0.2f, 0.2f);
            
            // Select Tool
            var selectBtn = CreateToolButton("Select", "1");
            selectBtn.Pressed += () => _toolManager?.SwitchToTool("Select");
            _toolbar.AddChild(selectBtn);
            
            // Brush Tool
            var brushBtn = CreateToolButton("Brush", "2");
            brushBtn.Pressed += () => _toolManager?.SwitchToTool("Brush");
            _toolbar.AddChild(brushBtn);
            
            // Eraser Tool
            var eraserBtn = CreateToolButton("Eraser", "3");
            eraserBtn.Pressed += () => _toolManager?.SwitchToTool("Eraser");
            _toolbar.AddChild(eraserBtn);
            
            // Move Tool
            var moveBtn = CreateToolButton("Move", "4");
            moveBtn.Pressed += () => _toolManager?.SwitchToTool("Move");
            _toolbar.AddChild(moveBtn);
            
            // Color Picker
            var pickerBtn = CreateToolButton("Picker", "5");
            pickerBtn.Pressed += () => _toolManager?.SwitchToTool("Color Picker");
            _toolbar.AddChild(pickerBtn);
            
            AddChild(_toolbar);
        }
        
        /// <summary>
        /// Create a tool button
        /// </summary>
        private Button CreateToolButton(string text, string shortcut)
        {
            var btn = new Button();
            btn.Text = $"{text} ({shortcut})";
            btn.CustomMinimumSize = new Vector2(120, 40);
            btn.Align = HorizontalAlignment.Center;
            return btn;
        }
        
        /// <summary>
        /// Create tool options panel
        /// </summary>
        private void CreateToolOptions()
        {
            _toolOptions = new HBoxContainer();
            _toolOptions.Name = "ToolOptions";
            
            // Color picker
            var colorLabel = new Label();
            colorLabel.Text = "Color:";
            _toolOptions.AddChild(colorLabel);
            
            _colorPicker = new ColorPickerButton();
            _colorPicker.Color = _primaryColor;
            _colorPicker.CustomMinimumSize = new Vector2(80, 40);
            _colorPicker.ColorChanged += OnColorChanged;
            _toolOptions.AddChild(_colorPicker);
            
            // Brush size
            var sizeLabel = new Label();
            sizeLabel.Text = "Size:";
            _toolOptions.AddChild(sizeLabel);
            
            _brushSizeSlider = new HSlider();
            _brushSizeSlider.MinValue = 1;
            _brushSizeSlider.MaxValue = 500;
            _brushSizeSlider.Step = 1;
            _brushSizeSlider.Value = 20;
            _brushSizeSlider.CustomMinimumSize = new Vector2(200, 40);
            _brushSizeSlider.ValueChanged += OnBrushSizeChanged;
            _toolOptions.AddChild(_brushSizeSlider);
            
            var sizeValueLabel = new Label();
            sizeValueLabel.Name = "SizeValue";
            sizeValueLabel.Text = "20";
            _toolOptions.AddChild(sizeValueLabel);
            
            // Opacity
            var opacityLabel = new Label();
            opacityLabel.Text = "Opacity:";
            _toolOptions.AddChild(opacityLabel);
            
            _opacitySlider = new HSlider();
            _opacitySlider.MinValue = 0;
            _opacitySlider.MaxValue = 1;
            _opacitySlider.Step = 0.01f;
            _opacitySlider.Value = 1;
            _opacitySlider.CustomMinimumSize = new Vector2(150, 40);
            _opacitySlider.ValueChanged += OnOpacityChanged;
            _toolOptions.AddChild(_opacitySlider);
            
            var opacityValueLabel = new Label();
            opacityValueLabel.Name = "OpacityValue";
            opacityValueLabel.Text = "100%";
            _toolOptions.AddChild(opacityValueLabel);
            
            AddChild(_toolOptions);
        }
        
        /// <summary>
        /// Create status bar
        /// </summary>
        private void CreateStatusBar()
        {
            _statusLabel = new Label();
            _statusLabel.Name = "StatusLabel";
            _statusLabel.Text = "Ready - Select a tool to start drawing";
            _statusLabel.HorizontalAlignment = HorizontalAlignment.Left;
            AddChild(_statusLabel);
        }
        
        /// <summary>
        /// Connect signals from managers
        /// </summary>
        private void ConnectSignals()
        {
            if (_toolManager != null)
            {
                _toolManager.ToolChanged += OnToolChanged;
            }
            
            if (_historyManager != null)
            {
                _historyManager.HistoryChanged += OnHistoryChanged;
            }
        }
        
        /// <summary>
        /// Update tool options based on current tool
        /// </summary>
        private void UpdateToolOptions()
        {
            if (_toolManager?.CurrentTool == null) return;
            
            var tool = _toolManager.CurrentTool;
            
            // Update sliders to match tool properties
            _brushSizeSlider.Value = tool.BrushSize;
            _opacitySlider.Value = tool.BrushOpacity;
        }
        
        // Event Handlers
        
        private void OnMenuIdPressed(long id)
        {
            switch (id)
            {
                case 0: // New
                    NewProject();
                    break;
                case 2: // Save
                    SaveProject();
                    break;
                case 3: // Export
                    ExportImage();
                    break;
                case 5: // Undo
                    _historyManager?.Undo();
                    break;
                case 6: // Redo
                    _historyManager?.Redo();
                    break;
                case 7: // Clear
                    _canvas?.ClearCanvas(Colors.White);
                    break;
                case 8: // Zoom In
                    _canvas?.ZoomIn(GetViewport().GetMousePosition());
                    break;
                case 9: // Zoom Out
                    _canvas?.ZoomOut(GetViewport().GetMousePosition());
                    break;
                case 10: // Reset Zoom
                    _canvas?.ResetView();
                    break;
                case 11: // Toggle Grid
                    if (_canvas != null)
                        _canvas.ShowGrid = !_canvas.ShowGrid;
                    break;
                case 12: // About
                    ShowAboutDialog();
                    break;
            }
        }
        
        private void OnToolChanged(BaseTool newTool)
        {
            UpdateStatus($"Active Tool: {newTool.ToolName} - {newTool.ToolDescription}");
            UpdateToolOptions();
            
            // Apply current color to brush tools
            if (newTool is BrushTool brush)
            {
                brush.BrushColor = _primaryColor;
            }
        }
        
        private void OnColorChanged(Color color)
        {
            _primaryColor = color;
            
            // Update current tool if it's a brush
            if (_toolManager?.CurrentTool is BrushTool brush)
            {
                brush.BrushColor = color;
            }
            
            UpdateStatus($"Color changed: {color.ToHtml()}");
        }
        
        private void OnBrushSizeChanged(double value)
        {
            float size = (float)value;
            
            if (_toolManager?.CurrentTool != null)
            {
                _toolManager.CurrentTool.BrushSize = size;
            }
            
            // Update size label
            var sizeLabel = _toolOptions.GetNodeOrNull<Label>("SizeValue");
            if (sizeLabel != null)
                sizeLabel.Text = ((int)size).ToString();
        }
        
        private void OnOpacityChanged(double value)
        {
            float opacity = (float)value;
            
            if (_toolManager?.CurrentTool != null)
            {
                _toolManager.CurrentTool.BrushOpacity = opacity;
            }
            
            // Update opacity label
            var opacityLabel = _toolOptions.GetNodeOrNull<Label>("OpacityValue");
            if (opacityLabel != null)
                opacityLabel.Text = $"{(int)(opacity * 100)}%";
        }
        
        private void OnHistoryChanged(int currentIndex, int totalStates)
        {
            UpdateStatus($"History: {currentIndex + 1}/{totalStates}");
        }
        
        // Actions
        
        private void NewProject()
        {
            _canvas?.InitializeCanvas(1920, 1080);
            UpdateStatus("New project created");
        }
        
        private void SaveProject()
        {
            string path = $"user://project_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            _canvas?.SaveToFile(path);
            UpdateStatus($"Project saved: {path}");
        }
        
        private void ExportImage()
        {
            string path = $"user://export_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            _canvas?.SaveToFile(path);
            UpdateStatus($"Image exported: {path}");
        }
        
        private void ShowAboutDialog()
        {
            UpdateStatus("PhotoGodot v1.0 - A powerful image editor built with Godot 4.3");
        }
        
        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
            }
            GD.Print($"[UI] {message}");
        }
        
        public override void _Input(InputEvent @event)
        {
            // Handle global shortcuts
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Number keys for tools
                if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key5)
                {
                    int toolIndex = (int)keyEvent.Keycode - (int)Key.Key1;
                    _toolManager?.SwitchToToolByIndex(toolIndex);
                }
            }
        }
    }
}
