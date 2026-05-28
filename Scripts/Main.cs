using Godot;
using System;
using PhotoGodot.Core;
using PhotoGodot.Tools;
using PhotoGodot.UI;

public partial class Main : Node2D
{
    // Core Managers
    private LayerManager _layerManager;
    private HistoryManager _historyManager;
    private ToolManager _toolManager;
    
    // UI Components
    private MainUI _mainUI;
    
    // Canvas de dibujo (Viewport para capturar input y dibujar)
    private ColorRect _canvasContainer;
    private Vector2 _canvasSize = new(1920, 1080);
    private Vector2 _canvasOffset = Vector2.Zero;
    private float _zoom = 1.0f;
    
    // Estado
    private Color _currentColor = Colors.Black;
    private float _brushSize = 10.0f;
    private float _brushHardness = 1.0f;
    private float _brushOpacity = 1.0f;
    private bool _showGrid = false;
    
    public LayerManager LayerManager => _layerManager;
    public ToolManager ToolManager => _toolManager;
    public HistoryManager HistoryManager => _historyManager;
    public Color CurrentColor { get => _currentColor; set => _currentColor = value; }
    public float BrushSize { get => _brushSize; set => _brushSize = value; }
    public float BrushHardness { get => _brushHardness; set => _brushHardness = value; }
    public float BrushOpacity { get => _brushOpacity; set => _brushOpacity = value; }
    public Vector2 CanvasOffset { get => _canvasOffset; set => _canvasOffset = value; }
    public float Zoom { get => _zoom; set => _zoom = value; }
    public bool ShowGrid { get => _showGrid; set => _showGrid = value; }
    public ColorRect CanvasContainer => _canvasContainer;

    public override void _Ready()
    {
        GD.Print("Iniciando PhotoGodot Pro...");
        
        // Configurar ventana
        var window = GetWindow();
        window.Size = new Vector2I(1280, 720);
        window.Title = "PhotoGodot Pro v2.0";
        
        // Crear contenedor principal
        var mainContainer = new VBoxContainer();
        AddChild(mainContainer);
        
        // Inicializar Managers
        _layerManager = new LayerManager();
        AddChild(_layerManager);
        
        _historyManager = new HistoryManager();
        AddChild(_historyManager);
        _historyManager.Setup(_layerManager);
        
        _toolManager = new ToolManager();
        AddChild(_toolManager);
        
        // Crear área de canvas (centro)
        _canvasContainer = new ColorRect();
        _canvasContainer.Color = new Color(0.15f, 0.15f, 0.15f); // Gris oscuro fondo
        _canvasContainer.CustomMinimumSize = new Vector2(800, 600);
        mainContainer.AddChild(_canvasContainer);
        
        // Crear UI completa
        _mainUI = new MainUI();
        AddChild(_mainUI);
        _mainUI.Setup(this);
        
        // Registrar herramientas
        RegisterTools();
        
        // Configurar lienzo inicial
        _layerManager.Setup((int)_canvasSize.X, (int)_canvasSize.Y, Colors.White);
        
        GD.Print("PhotoGodot Pro listo. Herramienta activa: " + _toolManager.CurrentTool?.ToolName);
    }

    private void RegisterTools()
    {
        var brush = new BrushTool();
        var eraser = new EraserTool();
        var picker = new ColorPickerTool();
        var move = new MoveTool();
        var select = new SelectTool();
        
        _toolManager.RegisterTool(brush);
        _toolManager.RegisterTool(eraser);
        _toolManager.RegisterTool(picker);
        _toolManager.RegisterTool(move);
        _toolManager.RegisterTool(select);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        // Manejar atajos globales
        if (@event is InputEventKey key && key.Pressed)
        {
            HandleGlobalShortcuts(key);
        }
        
        // Pasar input al tool manager si es sobre el canvas
        if (_canvasContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
        {
            _toolManager.HandleInput(@event);
        }
    }

    private void HandleGlobalShortcuts(InputEventKey key)
    {
        string keyCode = key.Keycode.ToString().ToLower();
        
        // Herramientas
        if (key.Keycode == Key.B) _toolManager.SetTool("Brush");
        else if (key.Keycode == Key.E) _toolManager.SetTool("Eraser");
        else if (key.Keycode == Key.I) _toolManager.SetTool("ColorPicker");
        else if (key.Keycode == Key.V) _toolManager.SetTool("Move");
        else if (key.Keycode == Key.M) _toolManager.SetTool("Select");
        else if (key.Keycode == Key.G) _showGrid = !_showGrid;
        
        // Undo/Redo
        if (key.CtrlPressed && key.Keycode == Key.Z)
        {
            if (key.ShiftPressed) _historyManager.Redo();
            else _historyManager.Undo();
        }
        
        // Acciones
        if (key.CtrlPressed && key.Keycode == Key.N) NewProject();
        if (key.CtrlPressed && key.Keycode == Key.S) SaveProject();
        if (key.CtrlPressed && key.Keycode == Key.E) ExportImage();
    }

    public void NewProject()
    {
        _historyManager.Clear();
        _layerManager.Setup((int)_canvasSize.X, (int)_canvasSize.Y, Colors.White);
        GD.Print("Nuevo proyecto creado");
    }

    public void SaveProject()
    {
        // Guardar configuración básica (en una versión completa se guardaría todo el estado)
        GD.Print("Proyecto guardado (simulado)");
    }

    public void ExportImage()
    {
        if (_layerManager.ActiveLayer == null) return;
        
        var image = _layerManager.ActiveLayer.ImageData.Duplicate();
        string path = $"user://export_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        Error err = image.SavePng(path);
        
        if (err == Error.Ok)
            GD.Print($"Imagen exportada a: {path}");
        else
            GD.PrintErr("Error al exportar imagen");
    }
    
    public Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        var rect = _canvasContainer.GetGlobalRect();
        var localPos = screenPos - rect.Position - _canvasOffset;
        return localPos / _zoom;
    }
    
    public void UpdateCanvasTransform()
    {
        // Notificar a la UI o componentes que necesitan redibujar
        _mainUI?.RefreshLayerPanel();
    }
}
