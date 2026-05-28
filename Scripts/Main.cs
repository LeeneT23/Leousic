using Godot;

namespace PhotoGodot;

public partial class Main : Node
{
    private Core.LayerManager? _layerManager;
    private Core.ToolManager? _toolManager;
    private Core.HistoryManager? _historyManager;
    private Core.DrawingCanvas? _canvas;
    private UI.MainUI? _mainUI;

    public override void _Ready()
    {
        GD.Print("===========================================");
        GD.Print("PhotoGodot Pro v1.0 - Iniciando...");
        GD.Print("Motor: Godot 4.6");
        GD.Print("Lenguaje: C#");
        GD.Print("===========================================");
        
        // Initialize core systems
        InitializeSystems();
        
        GD.Print("===========================================");
        GD.Print("¡PhotoGodot Pro listo para usar!");
        GD.Print("===========================================");
    }

    private void InitializeSystems()
    {
        // Create History Manager
        _historyManager = new Core.HistoryManager();
        _historyManager.Name = "HistoryManager";
        _historyManager.MaxHistorySize = 100;
        AddChild(_historyManager);
        GD.Print("[Main] HistoryManager initialized");
        
        // Create Layer Manager
        _layerManager = new Core.LayerManager();
        _layerManager.Name = "LayerManager";
        AddChild(_layerManager);
        GD.Print("[Main] LayerManager initialized");
        
        // Create Drawing Canvas
        _canvas = GetNode<Core.DrawingCanvas>("MainUI/HSplitContainer/CenterPanel");
        if (_canvas == null)
        {
            // Create canvas if not found in scene
            _canvas = new Core.DrawingCanvas();
            _canvas.Name = "DrawingCanvas";
            var centerPanel = GetNodeOrNull<Control>("MainUI/HSplitContainer/CenterPanel");
            if (centerPanel != null)
            {
                centerPanel.AddChild(_canvas);
            }
            else
            {
                GD.PrintErr("[Main] Could not find CenterPanel for canvas!");
                return;
            }
        }
        GD.Print("[Main] DrawingCanvas initialized");
        
        // Create Tool Manager
        _toolManager = new Core.ToolManager();
        _toolManager.Name = "ToolManager";
        AddChild(_toolManager);
        GD.Print("[Main] ToolManager initialized");
        
        // Get UI
        _mainUI = GetNode<UI.MainUI>("MainUI");
        if (_mainUI == null)
        {
            GD.PrintErr("[Main] Could not find MainUI!");
            return;
        }
        GD.Print("[Main] MainUI found");
        
        // Initialize canvas with managers
        _canvas.Initialize(_layerManager, _toolManager);
        
        // Initialize tool manager with canvas and managers
        _toolManager.Initialize(_canvas, _layerManager, _historyManager);
        
        // Initialize UI with all components
        _mainUI.Initialize(_layerManager, _toolManager, _historyManager, _canvas);
        
        // Set initial canvas size
        _canvas.SetCanvasSize(1024, 768);
        
        // Initialize layer manager with canvas size
        _layerManager.Initialize(1024, 768);
        
        GD.Print("[Main] All systems connected and ready");
    }

    public override void _Input(InputEvent @event)
    {
        // Handle global shortcuts
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Tool shortcuts
            if (!keyEvent.CtrlPressed && !keyEvent.AltPressed && !keyEvent.ShiftPressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.B:
                        _toolManager?.SelectTool("Brush");
                        GetViewport().SetInputAsHandled();
                        break;
                    case Key.E:
                        _toolManager?.SelectTool("Eraser");
                        GetViewport().SetInputAsHandled();
                        break;
                    case Key.I:
                        _toolManager?.SelectTool("ColorPicker");
                        GetViewport().SetInputAsHandled();
                        break;
                    case Key.V:
                        _toolManager?.SelectTool("Move");
                        GetViewport().SetInputAsHandled();
                        break;
                    case Key.M:
                        _toolManager?.SelectTool("Select");
                        GetViewport().SetInputAsHandled();
                        break;
                    case Key.G:
                        _canvas?.ToggleGrid();
                        GetViewport().SetInputAsHandled();
                        break;
                }
            }
            
            // Ctrl shortcuts handled by UI
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            GD.Print("[Main] Application closing...");
            // Save state or prompt user
            GetTree().Quit();
        }
    }
}
