using Godot;

public partial class Main : Node2D
{
    [Export] public int CanvasWidth { get; set; } = 1920;
    [Export] public int CanvasHeight { get; set; } = 1080;
    
    private DrawingCanvas _drawingCanvas;
    private LayerManager _layerManager;
    private ToolManager _toolManager;
    private HistoryManager _historyManager;
    private MainUI _mainUI;
    
    private Color _primaryColor = Colors.Black;
    private float _brushSize = 10.0f;
    private float _opacity = 1.0f;
    private float _hardness = 1.0f;
    
    public Color PrimaryColor 
    { 
        get => _primaryColor; 
        set 
        { 
            _primaryColor = value; 
            if (_mainUI != null) _mainUI.UpdateColorPreview(value);
        } 
    }
    
    public float BrushSize 
    { 
        get => _brushSize; 
        set => _brushSize = Mathf.Clamp(value, 1, 500); 
    }
    
    public float Opacity 
    { 
        get => _opacity; 
        set => _opacity = Mathf.Clamp(value, 0.01f, 1.0f); 
    }
    
    public float Hardness 
    { 
        get => _hardness; 
        set => _hardness = Mathf.Clamp(value, 0.0f, 1.0f); 
    }
    
    public override void _Ready()
    {
        InitializeCoreSystems();
        SetupUI();
        CreateDefaultLayer();
        SetupInputActions();
        
        GD.Print("🎨 PhotoGodot Pro initialized successfully!");
        GD.Print($"Canvas: {CanvasWidth}x{CanvasHeight}");
        GD.Print($"Layers: {_layerManager.LayerCount}");
    }
    
    private void InitializeCoreSystems()
    {
        _historyManager = new HistoryManager(this, 500);
        _layerManager = new LayerManager(this, CanvasWidth, CanvasHeight);
        _drawingCanvas = new DrawingCanvas(this);
        _toolManager = new ToolManager(this);
        
        AddChild(_historyManager);
        AddChild(_layerManager);
        AddChild(_drawingCanvas);
        AddChild(_toolManager);
        
        _toolManager.RegisterTool(new BrushTool(this));
        _toolManager.RegisterTool(new EraserTool(this));
        _toolManager.RegisterTool(new ColorPickerTool(this));
        _toolManager.RegisterTool(new MoveTool(this));
        _toolManager.RegisterTool(new SelectTool(this));
    }
    
    private void SetupUI()
    {
        var uiScene = GD.Load<PackedScene>("res://Scenes/MainUI.tscn");
        if (uiScene != null)
        {
            _mainUI = uiScene.Instantiate<MainUI>();
            AddChild(_mainUI);
            _mainUI.Initialize(this);
        }
        else
        {
            GD.PrintErr("Error: MainUI scene not found!");
            CreateFallbackUI();
        }
    }
    
    private void CreateFallbackUI()
    {
        _mainUI = new MainUI();
        AddChild(_mainUI);
        _mainUI.Initialize(this);
        GD.Print("Created fallback UI programmatically");
    }
    
    private void CreateDefaultLayer()
    {
        _layerManager.CreateLayer("Background");
        _layerManager.SetActiveLayer(0);
    }
    
    private void SetupInputActions()
    {
        // Input actions are defined in project.godot
        GD.Print("Input actions configured");
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            HandleKeyboardShortcuts(keyEvent);
        }
        
        if (_toolManager.CurrentTool != null)
        {
            _toolManager.CurrentTool.HandleInput(@event);
        }
    }
    
    private void HandleKeyboardShortcuts(InputEventKey keyEvent)
    {
        if (keyEvent.CtrlPressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Z:
                    _historyManager.Undo();
                    break;
                case Key.Y:
                    _historyManager.Redo();
                    break;
                case Key.N:
                    CreateNewDocument();
                    break;
                case Key.S:
                    SaveProject();
                    break;
                case Key.O:
                    OpenProject();
                    break;
            }
        }
        else
        {
            switch (keyEvent.Keycode)
            {
                case Key.B:
                    _toolManager.SetActiveTool("Brush");
                    break;
                case Key.E:
                    _toolManager.SetActiveTool("Eraser");
                    break;
                case Key.I:
                    _toolManager.SetActiveTool("ColorPicker");
                    break;
                case Key.V:
                    _toolManager.SetActiveTool("Move");
                    break;
                case Key.M:
                    _toolManager.SetActiveTool("Select");
                    break;
                case Key.G:
                    ToggleGrid();
                    break;
                case Key.Delete:
                    DeleteCurrentLayer();
                    break;
            }
        }
    }
    
    public void CreateNewDocument()
    {
        _layerManager.ClearAllLayers();
        _historyManager.Clear();
        CreateDefaultLayer();
        GD.Print("New document created");
    }
    
    public async void SaveProject()
    {
        var dialog = new FileDialog
        {
            Title = "Save Project",
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.png ; PNG Image", "*.jpg ; JPG Image" }
        };
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        await ToSignal(dialog, "file_selected");
        
        string path = dialog.FilePath;
        dialog.QueueFree();
        
        if (path.EndsWith(".png"))
        {
            _layerManager.ExportToPNG(path);
            GD.Print($"Project saved to: {path}");
        }
    }
    
    public async void OpenProject()
    {
        var dialog = new FileDialog
        {
            Title = "Open Image",
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.png, *.jpg, *.jpeg ; Image Files" }
        };
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        await ToSignal(dialog, "file_selected");
        
        string path = dialog.FilePath;
        dialog.QueueFree();
        
        LoadImage(path);
    }
    
    public void LoadImage(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            GD.PrintErr($"File not found: {path}");
            return;
        }
        
        var image = new Image();
        var error = image.Load(path);
        
        if (error == Error.Ok)
        {
            _layerManager.ClearAllLayers();
            _layerManager.CreateLayerFromImage(image, "Imported Layer");
            _layerManager.SetActiveLayer(0);
            GD.Print($"Image loaded: {path} ({image.GetWidth()}x{image.GetHeight()})");
        }
        else
        {
            GD.PrintErr($"Failed to load image: {error}");
        }
    }
    
    public void ToggleGrid()
    {
        if (_drawingCanvas != null)
        {
            _drawingCanvas.ToggleGrid();
        }
    }
    
    public void DeleteCurrentLayer()
    {
        if (_layerManager.ActiveLayerIndex >= 0)
        {
            _layerManager.DeleteLayer(_layerManager.ActiveLayerIndex);
        }
    }
    
    #region Public API for Tools and UI
    
    public DrawingCanvas GetDrawingCanvas() => _drawingCanvas;
    public LayerManager GetLayerManager() => _layerManager;
    public ToolManager GetToolManager() => _toolManager;
    public HistoryManager GetHistoryManager() => _historyManager;
    public MainUI GetMainUI() => _mainUI;
    
    public void SetPrimaryColor(Color color) => PrimaryColor = color;
    public Color GetPrimaryColor() => _primaryColor;
    
    public void SetBrushSize(float size) => BrushSize = size;
    public float GetBrushSize() => _brushSize;
    
    public void SetOpacity(float opacity) => Opacity = opacity;
    public float GetOpacity() => _opacity;
    
    public void SetHardness(float hardness) => Hardness = hardness;
    public float GetHardness() => _hardness;
    
    #endregion
}
