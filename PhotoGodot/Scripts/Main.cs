using Godot;
using PhotoGodot.Core;
using PhotoGodot.Tools;
using PhotoGodot.UI;

/// <summary>
/// Main scene controller for PhotoGodot application
/// This is the entry point that initializes all components
/// </summary>
public partial class Main : Node2D
{
    // Core managers
    private ToolManager _toolManager;
    private HistoryManager _historyManager;
    
    // Canvas and UI
    private DrawingCanvas _canvas;
    private MainUI _ui;
    
    public override void _Ready()
    {
        GD.Print("===========================================");
        GD.Print("PhotoGodot v1.0 - Starting...");
        GD.Print("===========================================");
        
        // Initialize core systems
        InitializeManagers();
        
        // Create canvas
        CreateCanvas();
        
        // Create UI
        CreateUI();
        
        // Register default tools
        RegisterDefaultTools();
        
        // Activate first tool by default
        _toolManager.SwitchToTool("Brush");
        
        GD.Print("===========================================");
        GD.Print("PhotoGodot Ready!");
        GD.Print("===========================================");
        GD.Print("Controls:");
        GD.Print("  1-5: Select tools (Select, Brush, Eraser, Move, Picker)");
        GD.Print("  Ctrl+S: Save project");
        GD.Print("  Ctrl+Z/Y: Undo/Redo");
        GD.Print("  Mouse Wheel: Zoom in/out");
        GD.Print("  Ctrl+G: Toggle grid");
        GD.Print("===========================================");
    }
    
    /// <summary>
    /// Initialize core manager nodes
    /// </summary>
    private void InitializeManagers()
    {
        // Create Tool Manager
        _toolManager = new ToolManager();
        _toolManager.Name = "ToolManager";
        AddChild(_toolManager);
        
        // Create History Manager
        _historyManager = new HistoryManager();
        _historyManager.Name = "HistoryManager";
        AddChild(_historyManager);
        
        GD.Print("[Main] Managers initialized");
    }
    
    /// <summary>
    /// Create the drawing canvas
    /// </summary>
    private void CreateCanvas()
    {
        _canvas = new DrawingCanvas();
        _canvas.Name = "DrawingCanvas";
        AddChild(_canvas);
        
        GD.Print("[Main] Canvas created");
    }
    
    /// <summary>
    /// Create the main UI
    /// </summary>
    private void CreateUI()
    {
        _ui = new MainUI();
        _ui.Name = "MainUI";
        AddChild(_ui);
        
        GD.Print("[Main] UI created");
    }
    
    /// <summary>
    /// Register all default tools
    /// </summary>
    private void RegisterDefaultTools()
    {
        // Create and register tools
        var selectTool = new SelectTool();
        _toolManager.RegisterTool(selectTool);
        
        var brushTool = new BrushTool();
        _toolManager.RegisterTool(brushTool);
        
        var eraserTool = new EraserTool();
        _toolManager.RegisterTool(eraserTool);
        
        var moveTool = new MoveTool();
        _toolManager.RegisterTool(moveTool);
        
        var colorPickerTool = new ColorPickerTool();
        _toolManager.RegisterTool(colorPickerTool);
        
        // Connect color picker signal
        colorPickerTool.ColorPicked += OnColorPicked;
        
        GD.Print($"[Main] Registered {_toolManager.GetAvailableTools().Count} tools");
    }
    
    /// <summary>
    /// Handle color picked from canvas
    /// </summary>
    private void OnColorPicked(Color color)
    {
        // Update UI color picker if available
        if (_ui != null)
        {
            // The UI will handle updating its own color picker
        }
        
        GD.Print($"[Main] Color picked: {color.ToHtml()}");
    }
    
    public override void _Process(double delta)
    {
        // Main process loop - managers handle their own processing
    }
    
    public override void _Input(InputEvent @event)
    {
        // Global input handling
        
        // Quit on Escape
        if (@event is InputEventKey keyEvent && 
            keyEvent.Pressed && 
            keyEvent.Keycode == Key.Escape)
        {
            GetTree().Quit();
        }
    }
    
    /// <summary>
    /// Get the tool manager instance
    /// </summary>
    public ToolManager GetToolManager() => _toolManager;
    
    /// <summary>
    /// Get the history manager instance
    /// </summary>
    public HistoryManager GetHistoryManager() => _historyManager;
    
    /// <summary>
    /// Get the drawing canvas instance
    /// </summary>
    public DrawingCanvas GetCanvas() => _canvas;
}
