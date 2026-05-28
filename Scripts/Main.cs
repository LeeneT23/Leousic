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
    
    // Canvas & Rendering
    private ColorRect _canvasBackground;
    private Node2D _canvasContainer;
    private Sprite2D _gridSprite;
    private Texture2D _gridTexture;
    
    // State
    private Vector2I _canvasSize = new(1920, 1080);
    private float _zoom = 1.0f;
    private bool _showGrid = false;
    private Color _primaryColor = Colors.Black;
    private Color _secondaryColor = Colors.White;
    
    // Constants
    private const float MIN_ZOOM = 0.1f;
    private const float MAX_ZOOM = 10.0f;
    
    public LayerManager LayerManager => _layerManager;
    public HistoryManager HistoryManager => _historyManager;
    public ToolManager ToolManager => _toolManager;
    public Vector2I CanvasSize => _canvasSize;
    public float Zoom => _zoom;
    public Color PrimaryColor { get => _primaryColor; set => _primaryColor = value; }
    public Color SecondaryColor { get => _secondaryColor; set => _secondaryColor = value; }
    public bool ShowGrid => _showGrid;
    
    public override void _Ready()
    {
        GD.Print("🎨 PhotoGodot Pro v2.0 - Iniciando...");
        
        SetupCanvas();
        InitializeManagers();
        RegisterTools();
        CreateUI();
        SetupInputMap();
        
        GD.Print($"✅ Lienzo creado: {_canvasSize.X}x{_canvasSize.Y}");
        GD.Print("✅ Sistema listo para dibujar");
    }
    
    private void SetupCanvas()
    {
        // Fondo del canvas (área de trabajo)
        _canvasBackground = new ColorRect();
        _canvasBackground.Color = new Color(0.15f, 0.15f, 0.15f);
        _canvasBackground.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_canvasBackground);
        
        // Contenedor del canvas que se puede hacer zoom/pan
        _canvasContainer = new Node2D();
        _canvasContainer.Position = new Vector2(100, 50);
        AddChild(_canvasContainer);
        
        // Grid texture (se genera programáticamente)
        GenerateGridTexture();
        _gridSprite = new Sprite2D();
        _gridSprite.Texture = _gridTexture;
        _gridSprite.Visible = false;
        _canvasContainer.AddChild(_gridSprite);
    }
    
    private void GenerateGridTexture()
    {
        int gridSize = 50;
        int texSize = 512;
        var image = Image.Create(texSize, texSize, false, Image.Format.Rgba8);
        image.Fill(new Color(0, 0, 0, 0));
        
        for (int x = 0; x < texSize; x += gridSize)
        {
            for (int y = 0; y < texSize; y++)
            {
                if (y % gridSize < 2)
                    image.SetPixel(x, y, new Color(1, 1, 1, 0.3f));
            }
        }
        for (int y = 0; y < texSize; y += gridSize)
        {
            for (int x = 0; x < texSize; x++)
            {
                if (x % gridSize < 2)
                    image.SetPixel(x, y, new Color(1, 1, 1, 0.3f));
            }
        }
        
        _gridTexture = ImageTexture.CreateFromImage(image);
    }
    
    private void InitializeManagers()
    {
        // Layer Manager
        _layerManager = new LayerManager();
        _layerManager.Name = "LayerManager";
        AddChild(_layerManager);
        _layerManager.Setup(_canvasSize.X, _canvasSize.Y, Colors.White);
        
        // History Manager
        _historyManager = new HistoryManager();
        _historyManager.Name = "HistoryManager";
        AddChild(_historyManager);
        _historyManager.Setup(_layerManager);
        
        // Tool Manager
        _toolManager = new ToolManager();
        _toolManager.Name = "ToolManager";
        AddChild(_toolManager);
        
        // Conectar señales
        _layerManager.LayerListChanged += OnLayerListChanged;
        _layerManager.ActiveLayerChanged += OnActiveLayerChanged;
    }
    
    private void RegisterTools()
    {
        var brush = new BrushTool();
        brush.ToolName = "Pincel";
        brush.ShortcutKey = "b";
        _toolManager.RegisterTool(brush);
        
        var eraser = new EraserTool();
        eraser.ToolName = "Borrador";
        eraser.ShortcutKey = "e";
        _toolManager.RegisterTool(eraser);
        
        var picker = new ColorPickerTool();
        picker.ToolName = "Selector";
        picker.ShortcutKey = "i";
        _toolManager.RegisterTool(picker);
        
        var move = new MoveTool();
        move.ToolName = "Mover";
        move.ShortcutKey = "v";
        _toolManager.RegisterTool(move);
        
        var select = new SelectTool();
        select.ToolName = "Selección";
        select.ShortcutKey = "m";
        _toolManager.RegisterTool(select);
        
        GD.Print($"✅ {_toolManager.GetToolByName("Pincel")?.ToolName}, {_toolManager.GetToolByName("Borrador")?.ToolName}, etc. registradas");
    }
    
    private void CreateUI()
    {
        _mainUI = new MainUI();
        _mainUI.Initialize(this);
        AddChild(_mainUI);
        GD.Print("✅ Interfaz de usuario creada");
    }
    
    private void SetupInputMap()
    {
        // Los atajos ya están definidos en project.godot
        GD.Print("✅ Mapa de entrada configurado desde project.godot");
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            HandleKeyboardInput(key);
        }
        
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseInput(mouseButton);
        }
        
        if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMotion(mouseMotion);
        }
        
        // Pasar input al tool manager
        _toolManager.HandleInput(@event);
    }
    
    private void HandleKeyboardInput(InputEventKey key)
    {
        // Atajos globales
        if (key.CtrlPressed && key.Keycode == Key.Z)
        {
            if (key.ShiftPressed)
                _historyManager.Redo();
            else
                _historyManager.Undo();
            return;
        }
        
        if (key.CtrlPressed && key.Keycode == Key.S)
        {
            ExportImage();
            return;
        }
        
        if (key.CtrlPressed && key.Keycode == Key.N)
        {
            NewFile();
            return;
        }
        
        if (key.CtrlPressed && key.Keycode == Key.E)
        {
            ExportImage();
            return;
        }
        
        if (key.CtrlPressed && key.Keycode == Key.Equal || key.CtrlPressed && key.Keycode == Key.Plus)
        {
            SetZoom(_zoom + 0.1f);
            return;
        }
        
        if (key.CtrlPressed && key.Keycode == Key.Minus)
        {
            SetZoom(_zoom - 0.1f);
            return;
        }
        
        if (key.Keycode == Key.G)
        {
            ToggleGrid();
            return;
        }
        
        // Cambiar herramientas con teclas
        if (!key.CtrlPressed && !key.AltPressed && !key.ShiftPressed)
        {
            string keyCode = ((char)key.Unicode).ToString().ToLower();
            
            switch (keyCode)
            {
                case "b":
                    _toolManager.SetTool("Pincel");
                    break;
                case "e":
                    _toolManager.SetTool("Borrador");
                    break;
                case "i":
                    _toolManager.SetTool("Selector");
                    break;
                case "v":
                    _toolManager.SetTool("Mover");
                    break;
                case "m":
                    _toolManager.SetTool("Selección");
                    break;
            }
        }
    }
    
    private void HandleMouseInput(InputEventMouseButton mouseButton)
    {
        Vector2 canvasPos = GetCanvasPosition(mouseButton.Position);
        
        if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.CtrlPressed)
        {
            SetZoom(_zoom + 0.1f);
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.CtrlPressed)
        {
            SetZoom(_zoom - 0.1f);
        }
        else if (mouseButton.ButtonIndex == MouseButton.Middle)
        {
            // Pan con rueda central (se implementaría con estado)
        }
    }
    
    private void HandleMouseMotion(InputEventMouseMotion mouseMotion)
    {
        // Actualizar posición del cursor en UI si es necesario
        _mainUI?.UpdateCursorPosition(GetCanvasPosition(mouseMotion.Position));
    }
    
    public Vector2 GetCanvasPosition(Vector2 screenPos)
    {
        return (screenPos - _canvasContainer.Position) / _zoom;
    }
    
    public void SetZoom(float newZoom)
    {
        _zoom = Math.Clamp(newZoom, MIN_ZOOM, MAX_ZOOM);
        _canvasContainer.Scale = new Vector2(_zoom, _zoom);
        _mainUI?.UpdateZoomDisplay(_zoom);
        GD.Print($"Zoom: {_zoom:P0}");
    }
    
    public void ToggleGrid()
    {
        _showGrid = !_showGrid;
        _gridSprite.Visible = _showGrid;
        GD.Print($"Grid: {(_showGrid ? "ON" : "OFF")}");
    }
    
    public void NewFile()
    {
        // Limpiar historial
        _historyManager.Clear();
        
        // Reiniciar capas
        _layerManager.QueueFree();
        _layerManager = new LayerManager();
        _layerManager.Name = "LayerManager";
        AddChild(_layerManager);
        _layerManager.Setup(_canvasSize.X, _canvasSize.Y, Colors.White);
        _historyManager.Setup(_layerManager);
        
        // Reconectar señales
        _layerManager.LayerListChanged += OnLayerListChanged;
        _layerManager.ActiveLayerChanged += OnActiveLayerChanged;
        
        // Re-registrar herramientas con nuevo manager
        _toolManager.QueueFree();
        _toolManager = new ToolManager();
        _toolManager.Name = "ToolManager";
        AddChild(_toolManager);
        RegisterTools();
        
        GD.Print("📄 Nuevo archivo creado");
    }
    
    public void ExportImage()
    {
        var image = _layerManager.GetCompositedImage();
        if (image != null)
        {
            string path = $"user://export_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            image.SavePng(path);
            GD.Print($"💾 Imagen exportada: {path}");
            
            // Mostrar notificación en UI
            _mainUI?.ShowNotification("Imagen exportada exitosamente");
        }
    }
    
    private void OnLayerListChanged()
    {
        _mainUI?.UpdateLayerList();
    }
    
    private void OnActiveLayerChanged(Layer layer)
    {
        _mainUI?.UpdateActiveLayer(layer);
    }
    
    public void UpdateGridVisibility()
    {
        _gridSprite.Visible = _showGrid;
    }
}
