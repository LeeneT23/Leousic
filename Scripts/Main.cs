using Godot;
using System;
using System.IO;

/// <summary>
/// Script principal de PhotoGodot Pro.
/// Punto de entrada que inicializa toda la aplicación.
/// </summary>
public partial class Main : Node
{
    // Componentes principales
    private DrawingCanvas? _canvas;
    private ToolManager? _toolManager;
    private LayerManager? _layerManager;
    private HistoryManager? _historyManager;
    private MainUI? _ui;
    
    // Ruta del proyecto actual
    private string? _currentFilePath;
    private bool _isModified = false;
    
    public override void _Ready()
    {
        GD.Print("===========================================");
        GD.Print("   PhotoGodot Pro - Editor de Imágenes");
        GD.Print("   Versión 1.0 - Godot 4.6");
        GD.Print("===========================================");
        
        InitializeComponents();
        SetupConnections();
        LoadDefaultSettings();
        
        GD.Print("[Main] Aplicación lista");
    }
    
    /// <summary>
    /// Inicializa todos los componentes de la aplicación
    /// </summary>
    private void InitializeComponents()
    {
        // Crear nodo de canvas
        _canvas = new DrawingCanvas();
        _canvas.Name = "DrawingCanvas";
        AddChild(_canvas);
        
        // Crear gestor de herramientas
        _toolManager = new ToolManager();
        _toolManager.Name = "ToolManager";
        _toolManager.Canvas = _canvas;
        _toolManager.UI = _ui;
        AddChild(_toolManager);
        
        // Crear gestor de capas
        _layerManager = new LayerManager();
        _layerManager.Name = "LayerManager";
        _layerManager.Canvas = _canvas;
        AddChild(_layerManager);
        
        // Crear gestor de historial
        _historyManager = new HistoryManager();
        _historyManager.Name = "HistoryManager";
        _historyManager.Canvas = _canvas;
        AddChild(_historyManager);
        
        // Obtener referencia a la UI (asumiendo que está en la escena)
        _ui = GetNodeOrNull<MainUI>("MainUI");
        
        // Configurar referencias cruzadas
        if (_ui != null)
        {
            _ui.SetManagers(_toolManager, _layerManager, _historyManager, _canvas);
        }
        
        _toolManager.UI = _ui;
        _canvas.SetManagers(_layerManager, _toolManager, _historyManager);
        
        // Ajustar canvas a la vista
        _canvas?.FitToView();
        
        GD.Print("[Main] Componentes inicializados");
    }
    
    /// <summary>
    /// Configura las conexiones entre componentes
    /// </summary>
    private void SetupConnections()
    {
        // Conectar señales de la UI
        if (_ui != null)
        {
            _ui.NewDocumentRequested += OnNewDocumentRequested;
            _ui.SaveRequested += OnSaveRequested;
            _ui.ExportRequested += OnExportRequested;
        }
        
        // Conectar señales del gestor de herramientas
        if (_toolManager != null)
        {
            _toolManager.ToolChanged += OnToolChanged;
        }
        
        // Conectar señales del gestor de capas
        if (_layerManager != null)
        {
            _layerManager.LayerAdded += (layer) => UpdateWindowTitle();
            _layerManager.LayerRemoved += (id) => UpdateWindowTitle();
        }
        
        // Conectar señales del historial
        if (_historyManager != null)
        {
            _historyManager.HistoryChanged += (current, total) => 
            {
                _isModified = current > 0;
                UpdateWindowTitle();
            };
        }
    }
    
    /// <summary>
    /// Carga configuraciones por defecto
    /// </summary>
    private void LoadDefaultSettings()
    {
        // Activar herramienta pincel por defecto
        _toolManager?.ActivateTool("BrushTool");
        
        // Crear capa inicial
        _layerManager?.CreateLayer("Fondo");
        
        UpdateWindowTitle();
    }
    
    #region Input Handling
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        // Procesar atajos de teclado globales
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            HandleKeyboardShortcuts(keyEvent);
        }
        
        // Pasar input al tool manager
        _toolManager?.ProcessInput(@event);
    }
    
    /// <summary>
    /// Maneja los atajos de teclado
    /// </summary>
    private void HandleKeyboardShortcuts(InputEventKey keyEvent)
    {
        bool ctrl = Input.IsKeyPressed(Key.Ctrl);
        bool shift = Input.IsKeyPressed(Key.Shift);
        bool alt = Input.IsKeyPressed(Key.Alt);
        
        // Ctrl + Z = Undo
        if (ctrl && !shift && keyEvent.Keycode == Key.Z)
        {
            _historyManager?.Undo();
            keyEvent.Handled = true;
            return;
        }
        
        // Ctrl + Shift + Z o Ctrl + Y = Redo
        if (ctrl && (shift || keyEvent.Keycode == Key.Y))
        {
            _historyManager?.Redo();
            keyEvent.Handled = true;
            return;
        }
        
        // Ctrl + S = Guardar
        if (ctrl && keyEvent.Keycode == Key.S)
        {
            OnSaveRequested();
            keyEvent.Handled = true;
            return;
        }
        
        // Ctrl + N = Nuevo documento
        if (ctrl && keyEvent.Keycode == Key.N)
        {
            OnNewDocumentRequested(1920, 1080);
            keyEvent.Handled = true;
            return;
        }
        
        // Ctrl + E = Exportar
        if (ctrl && keyEvent.Keycode == Key.E)
        {
            OnExportRequested("export.png", "png");
            keyEvent.Handled = true;
            return;
        }
        
        // Ctrl + L = Nueva capa
        if (ctrl && keyEvent.Keycode == Key.L)
        {
            _ui?.OnNewLayer();
            keyEvent.Handled = true;
            return;
        }
        
        // G = Toggle Grid
        if (keyEvent.Keycode == Key.G && !ctrl && !alt)
        {
            _ui?.OnToggleGrid();
            keyEvent.Handled = true;
            return;
        }
        
        // B = Pincel
        if (keyEvent.Keycode == Key.B && !ctrl)
        {
            _ui?.OnSelectBrush();
            keyEvent.Handled = true;
            return;
        }
        
        // E = Borrador
        if (keyEvent.Keycode == Key.E && !ctrl)
        {
            _ui?.OnSelectEraser();
            keyEvent.Handled = true;
            return;
        }
        
        // I = Selector de color
        if (keyEvent.Keycode == Key.I && !ctrl)
        {
            _ui?.OnSelectColorPicker();
            keyEvent.Handled = true;
            return;
        }
        
        // V = Mover
        if (keyEvent.Keycode == Key.V && !ctrl)
        {
            _ui?.OnSelectMove();
            keyEvent.Handled = true;
            return;
        }
        
        // M = Selección
        if (keyEvent.Keycode == Key.M && !ctrl)
        {
            _ui?.OnSelectSelect();
            keyEvent.Handled = true;
            return;
        }
        
        // Espacio = Mano (paneo)
        if (keyEvent.Keycode == Key.Space)
        {
            Input.SetCustomMouseCursor(CursorShape.CursorMove);
            keyEvent.Handled = true;
            return;
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnToolChanged(string toolName)
    {
        GD.Print($"[Main] Herramienta cambiada a: {toolName}");
    }
    
    private void OnNewDocumentRequested(int width, int height)
    {
        if (_isModified)
        {
            // En una implementación real, preguntaría si quiere guardar
            GD.Print("[Main] Documento modificado sin guardar");
        }
        
        // Limpiar todo
        _historyManager?.Clear();
        _layerManager?.ClearAllLayers();
        
        // Actualizar tamaño del canvas si es diferente
        if (_canvas != null && (_canvas.CanvasWidth != width || _canvas.CanvasHeight != height))
        {
            _canvas.CanvasWidth = width;
            _canvas.CanvasHeight = height;
            GD.Print($"[Main] Tamaño del canvas cambiado a: {width}x{height}");
        }
        
        // Crear capa inicial
        _layerManager?.CreateLayer("Fondo");
        
        _currentFilePath = null;
        _isModified = false;
        UpdateWindowTitle();
        
        GD.Print("[Main] Nuevo documento creado");
    }
    
    private void OnSaveRequested()
    {
        if (_canvas == null || _layerManager == null)
            return;
        
        // En Godot 4, guardamos como archivo de proyecto personalizado
        string savePath = _currentFilePath ?? "user://project.pgd";
        
        var data = new Dictionary<string, Variant>
        {
            { "version", 1 },
            { "canvas_width", _canvas.CanvasWidth },
            { "canvas_height", _canvas.CanvasHeight },
            { "layers", SerializeLayers() }
        };
        
        string jsonString = Json.Stringify(data);
        
        try
        {
            using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(jsonString);
                _currentFilePath = savePath;
                _isModified = false;
                UpdateWindowTitle();
                GD.Print($"[Main] Proyecto guardado en: {savePath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Main] Error al guardar: {ex.Message}");
        }
    }
    
    private void OnExportRequested(string path, string format)
    {
        if (_canvas == null)
            return;
        
        Image exportImage = _canvas.ExportAsImage();
        
        if (exportImage.GetSize().X == 0)
        {
            GD.PrintErr("[Main] No hay imagen para exportar");
            return;
        }
        
        string fullPath = $"user://{path}";
        
        Error error = format.ToLower() switch
        {
            "png" => exportImage.SavePng(fullPath),
            "jpg" or "jpeg" => exportImage.SaveJpg(fullPath),
            "webp" => exportImage.SaveWebp(fullPath),
            _ => exportImage.SavePng(fullPath)
        };
        
        if (error == Error.Ok)
        {
            GD.Print($"[Main] Imagen exportada a: {fullPath}");
        }
        else
        {
            GD.PrintErr($"[Main] Error al exportar: {error}");
        }
    }
    
    #endregion
    
    #region Serialization
    
    /// <summary>
    /// Serializa las capas a un array JSON
    /// </summary>
    private Variant[] SerializeLayers()
    {
        if (_layerManager == null)
            return Array.Empty<Variant>();
        
        var layers = _layerManager.GetAllLayers();
        var result = new Variant[layers.Count];
        
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            var layerData = new Dictionary<string, Variant>
            {
                { "id", layer.Id },
                { "name", layer.Name },
                { "visible", layer.Visible },
                { "opacity", layer.Opacity },
                { "blend_mode", (int)layer.BlendMode },
                { "offset_x", layer.Offset.X },
                { "offset_y", layer.Offset.Y }
                // La textura se guarda en un archivo separado
            };
            
            // Guardar textura como archivo PNG
            if (layer.Texture != null)
            {
                string texturePath = $"user://layer_{layer.Id}.png";
                Image img = layer.Texture.GetImage();
                img.SavePng(texturePath);
                layerData["texture_path"] = texturePath;
            }
            
            result[i] = Variant.From(layerData);
        }
        
        return result;
    }
    
    #endregion
    
    /// <summary>
    /// Actualiza el título de la ventana
    /// </summary>
    private void UpdateWindowTitle()
    {
        string fileName = _currentFilePath != null ? System.IO.Path.GetFileName(_currentFilePath) : "Sin título";
        string modifiedMarker = _isModified ? " *" : "";
        
        DisplayServer.WindowSetTitle($"PhotoGodot Pro - {fileName}{modifiedMarker}");
    }
    
    public override void _ExitTree()
    {
        if (_isModified)
        {
            GD.Print("[Main] Advertencia: Hay cambios sin guardar");
        }
        
        GD.Print("[Main] PhotoGodot Pro cerrado");
    }
}
