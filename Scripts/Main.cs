using Godot;
using PhotoGodot.Core;
using PhotoGodot.Tools;
using PhotoGodot.UI;

public partial class Main : Node2D
{
    // Componentes principales
    private LayerManager _layerManager;
    private HistoryManager _historyManager;
    private ToolManager _toolManager;
    private MainUI _mainUI;
    
    // Canvas de dibujo
    private ColorRect _canvasBackground;
    private Node2D _canvasContainer;
    private ZoomContainer _zoomContainer;
    
    // Estado
    private Color _currentColor = Colors.Black;
    private float _zoom = 1.0f;
    private bool _showGrid = false;
    private Line2D _selectionLine;
    
    // Constantes
    private const int CanvasWidth = 1920;
    private const int CanvasHeight = 1080;

    public override void _Ready()
    {
        // Configurar ventana
        DisplayServer.WindowSetTitle("PhotoGodot Pro");
        
        // Crear contenedores principales
        CreateCanvasSystem();
        
        // Inicializar managers
        InitializeManagers();
        
        // Crear UI
        _mainUI = new MainUI();
        AddChild(_mainUI);
        _mainUI.Setup(this, _layerManager, _toolManager, _historyManager);
        
        // Registrar herramientas
        RegisterTools();
        
        // Configurar selección
        _selectionLine = new Line2D();
        _selectionLine.DefaultColor = Colors.Yellow;
        _selectionLine.Width = 2.0f;
        _selectionLine.Visible = false;
        _canvasContainer.AddChild(_selectionLine);
        
        GD.Print("PhotoGodot Pro iniciado correctamente");
    }

    private void CreateCanvasSystem()
    {
        // Contenedor principal con zoom
        _zoomContainer = new ZoomContainer();
        _zoomContainer.Name = "ZoomContainer";
        _zoomContainer.AnchorRight = 1.0f;
        _zoomContainer.AnchorBottom = 1.0f;
        _zoomContainer.OffsetRight = -250; // Espacio para panel derecho
        _zoomContainer.OffsetBottom = -50; // Espacio para barra inferior
        AddChild(_zoomContainer);
        
        // Contenedor del canvas
        _canvasContainer = new Node2D();
        _canvasContainer.Name = "CanvasContainer";
        _canvasContainer.Position = new Vector2(100, 100);
        _zoomContainer.AddChild(_canvasContainer);
        
        // Fondo del canvas (blanco)
        _canvasBackground = new ColorRect();
        _canvasBackground.Color = Colors.White;
        _canvasBackground.SetSize(new Vector2(CanvasWidth, CanvasHeight));
        _canvasContainer.AddChild(_canvasBackground);
    }

    private void InitializeManagers()
    {
        // Layer Manager
        _layerManager = new LayerManager();
        _layerManager.Name = "LayerManager";
        _canvasContainer.AddChild(_layerManager);
        _layerManager.Setup(CanvasWidth, CanvasHeight, Colors.White);
        
        // History Manager
        _historyManager = new HistoryManager();
        _historyManager.Name = "HistoryManager";
        AddChild(_historyManager);
        _historyManager.Setup(_layerManager);
        
        // Tool Manager
        _toolManager = new ToolManager();
        _toolManager.Name = "ToolManager";
        AddChild(_toolManager);
    }

    private void RegisterTools()
    {
        _toolManager.RegisterTool(new BrushTool());
        _toolManager.RegisterTool(new EraserTool());
        _toolManager.RegisterTool(new ColorPickerTool());
        _toolManager.RegisterTool(new MoveTool());
        _toolManager.RegisterTool(new SelectTool());
    }

    public override void _Input(InputEvent @event)
    {
        // Pasar input a la herramienta activa
        _toolManager.HandleInput(@event);
        
        // Atajos de teclado globales
        if (@event is InputEventKey key && key.Pressed)
        {
            HandleKeyboardShortcuts(key);
        }
        
        // Zoom con rueda del ratón + Ctrl
        if (@event is InputEventMouseButton mb && mb.CtrlPressed)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp)
                SetZoom(_zoom * 1.1f);
            else if (mb.ButtonIndex == MouseButton.WheelDown)
                SetZoom(_zoom / 1.1f);
        }
    }

    private void HandleKeyboardShortcuts(InputEventKey key)
    {
        string keyCode = key.Keycode.ToString().ToLower();
        
        // Herramientas
        if (key.Keycode == Key.B) _toolManager.SetTool("Pincel");
        else if (key.Keycode == Key.E) _toolManager.SetTool("Borrador");
        else if (key.Keycode == Key.I) _toolManager.SetTool("Selector");
        else if (key.Keycode == Key.V) _toolManager.SetTool("Mover");
        else if (key.Keycode == Key.M) _toolManager.SetTool("Seleccionar");
        else if (key.Keycode == Key.G) ToggleGrid();
        
        // Acciones
        else if (key.Keycode == Key.Z && key.CtrlPressed && !key.ShiftPressed)
            _historyManager.Undo();
        else if (key.Keycode == Key.Z && key.CtrlPressed && key.ShiftPressed)
            _historyManager.Redo();
        else if (key.Keycode == Key.S && key.CtrlPressed)
            ExportImage();
        else if (key.Keycode == Key.N && key.CtrlPressed)
            NewDocument();
        else if (key.Keycode == Key.Plus && key.CtrlPressed)
            SetZoom(_zoom * 1.2f);
        else if (key.Keycode == Key.Minus && key.CtrlPressed)
            SetZoom(_zoom / 1.2f);
    }

    public Vector2 GetCanvasPosition(Vector2 screenPos)
    {
        return screenPos / _zoom - _canvasContainer.Position;
    }

    public void SetCurrentColor(Color color)
    {
        _currentColor = color;
        _mainUI?.UpdateColorPicker(color);
        
        // Actualizar color en herramienta pincel
        var brush = _toolManager.GetToolByName("Pincel") as BrushTool;
        if (brush != null)
            brush.BrushColor = color;
    }

    public Color GetCurrentColor() => _currentColor;

    public void SetZoom(float zoom)
    {
        _zoom = Mathf.Clamp(zoom, 0.1f, 10.0f);
        _canvasContainer.Scale = new Vector2(_zoom, _zoom);
        _mainUI?.UpdateZoomLabel(_zoom);
    }

    public void ToggleGrid()
    {
        _showGrid = !_showGrid;
        // Aquí se podría dibujar una grid si se desea
        GD.Print($"Grid: {(_showGrid ? "Activado" : "Desactivado")}");
    }

    public void SetCursor(string cursorType)
    {
        // Godot maneja cursores automáticamente según el contexto
    }

    public void ShowSelection(bool show)
    {
        _selectionLine.Visible = show;
    }

    public void UpdateSelection(Rect2 selection)
    {
        if (selection.Size.X == 0 || selection.Size.Y == 0)
        {
            _selectionLine.Visible = false;
            return;
        }
        
        _selectionLine.Visible = true;
        _selectionLine.ClearPoints();
        
        Vector2 topLeft = selection.Position;
        Vector2 topRight = new Vector2(selection.End.X, selection.Position.Y);
        Vector2 bottomRight = selection.End;
        Vector2 bottomLeft = new Vector2(selection.Position.X, selection.End.Y);
        
        _selectionLine.AddPoint(topLeft);
        _selectionLine.AddPoint(topRight);
        _selectionLine.AddPoint(bottomRight);
        _selectionLine.AddPoint(bottomLeft);
        _selectionLine.AddPoint(topLeft);
    }

    public void ExportImage()
    {
        if (_layerManager.ActiveLayer == null) return;
        
        string path = $"user://export_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        _layerManager.ActiveLayer.ImageData.SavePng(path);
        GD.Print($"Imagen exportada: {path}");
    }

    public void NewDocument()
    {
        _historyManager.Clear();
        _layerManager.ClearAll();
        _layerManager.Setup(CanvasWidth, CanvasHeight, Colors.White);
        GD.Print("Nuevo documento creado");
    }

    public LayerManager GetLayerManager() => _layerManager;
    public ToolManager GetToolManager() => _toolManager;
    public HistoryManager GetHistoryManager() => _historyManager;
}

// Contenedor personalizado para zoom
public partial class ZoomContainer : Control
{
    public override void _GuiInput(InputEvent @event)
    {
        // Manejar paneo con rueda central
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Middle)
        {
            if (mb.Pressed)
            {
                // Iniciar paneo
            }
            else
            {
                // Terminar paneo
            }
        }
        else if (@event is InputEventMouseMotion mm)
        {
            // Mover durante paneo
        }
    }
}
