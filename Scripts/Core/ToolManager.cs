using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestiona el ciclo de vida y la selección de herramientas.
/// Permite registrar herramientas personalizadas dinámicamente.
/// </summary>
public partial class ToolManager : Node
{
    [Signal] public delegate void ToolChangedEventHandler(string toolName);
    
    private Dictionary<string, BaseTool> _tools = new();
    private BaseTool? _currentTool;
    
    [Export] public DrawingCanvas? Canvas { get; set; }
    [Export] public MainUI? UI { get; set; }
    
    public override void _Ready()
    {
        RegisterBuiltInTools();
    }
    
    /// <summary>
    /// Registra las herramientas incorporadas por defecto
    /// </summary>
    private void RegisterBuiltInTools()
    {
        RegisterTool(new BrushTool());
        RegisterTool(new EraserTool());
        RegisterTool(new ColorPickerTool());
        RegisterTool(new MoveTool());
        RegisterTool(new SelectTool());
        
        GD.Print($"[ToolManager] { _tools.Count} herramientas registradas");
    }
    
    /// <summary>
    /// Registra una nueva herramienta (puede ser personalizada)
    /// </summary>
    public void RegisterTool(BaseTool tool)
    {
        if (tool == null)
        {
            GD.PrintErr("[ToolManager] Intento de registrar herramienta nula");
            return;
        }
        
        string toolId = tool.GetType().Name;
        
        if (_tools.ContainsKey(toolId))
        {
            GD.Print($"[ToolManager] Actualizando herramienta: {tool.ToolName}");
            _tools[toolId] = tool;
        }
        else
        {
            _tools.Add(toolId, tool);
            GD.Print($"[ToolManager] Nueva herramienta registrada: {tool.ToolName}");
        }
        
        tool.Initialize(Canvas!, UI!);
        AddChild(tool);
        
        // Notificar a la UI si es necesario
        if (UI != null)
        {
            UI.OnToolRegistered(tool);
        }
    }
    
    /// <summary>
    /// Activa una herramienta por su ID
    /// </summary>
    public void ActivateTool(string toolId)
    {
        if (!_tools.ContainsKey(toolId))
        {
            GD.PrintErr($"[ToolManager] Herramienta no encontrada: {toolId}");
            return;
        }
        
        // Desactivar herramienta actual
        if (_currentTool != null && _currentTool.GetType().Name != toolId)
        {
            _currentTool.OnDeactivate();
        }
        
        // Activar nueva herramienta
        _currentTool = _tools[toolId];
        _currentTool.OnActivate();
        
        EmitSignal(SignalName.ToolChanged, _currentTool.ToolName);
        GD.Print($"[ToolManager] Herramienta activa: {_currentTool.ToolName}");
    }
    
    /// <summary>
    /// Activa una herramienta por instancia
    /// </summary>
    public void ActivateTool(BaseTool tool)
    {
        string toolId = tool.GetType().Name;
        ActivateTool(toolId);
    }
    
    /// <summary>
    /// Obtiene la herramienta actual
    /// </summary>
    public BaseTool? GetCurrentTool()
    {
        return _currentTool;
    }
    
    /// <summary>
    /// Obtiene todas las herramientas registradas
    /// </summary>
    public Dictionary<string, BaseTool> GetAllTools()
    {
        return new Dictionary<string, BaseTool>(_tools);
    }
    
    /// <summary>
    /// Procesa el input para la herramienta actual
    /// </summary>
    public void ProcessInput(InputEvent @event)
    {
        if (_currentTool == null || Canvas == null)
            return;
        
        // Manejar eventos de mouse en el canvas
        if (@event is InputEventMouseButton mouseButton)
        {
            Vector2 canvasPos = GetCanvasPosition(mouseButton.Position);
            
            if (mouseButton.Pressed)
            {
                bool shift = Input.IsKeyPressed(Key.Shift);
                bool ctrl = Input.IsKeyPressed(Key.Ctrl);
                bool alt = Input.IsKeyPressed(Key.Alt);
                
                _currentTool.OnInputPressed(canvasPos, mouseButton.ButtonIndex, shift, ctrl, alt);
            }
            else
            {
                _currentTool.OnInputReleased(canvasPos, mouseButton.ButtonIndex);
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            Vector2 canvasPos = GetCanvasPosition(mouseMotion.Position);
            Vector2 delta = mouseMotion.Relative;
            
            _currentTool.OnInputDragged(canvasPos, delta, mouseMotion.ButtonMask);
        }
        
        // Procesar input general (teclado, etc.)
        _currentTool.ProcessInput(@event);
    }
    
    /// <summary>
    /// Convierte posición de pantalla a posición del canvas
    /// </summary>
    private Vector2 GetCanvasPosition(Vector2 screenPos)
    {
        if (Canvas == null)
            return screenPos;
        
        // Obtener la transformación del viewport
        Viewport viewport = GetViewport();
        if (viewport != null)
        {
            // Considerar zoom y paneo
            Vector2 offset = Canvas.GetGlobalTransform().Origin;
            float zoom = Canvas.CurrentZoom;
            
            return (screenPos - offset) / zoom;
        }
        
        return screenPos;
    }
    
    /// <summary>
    /// Actualiza los ajustes de la herramienta actual
    /// </summary>
    public void UpdateCurrentToolSettings(Dictionary<string, Variant> settings)
    {
        if (_currentTool != null)
        {
            _currentTool.ApplyToolSettings(settings);
        }
    }
    
    /// <summary>
    /// Obtiene los ajustes de la herramienta actual
    /// </summary>
    public Dictionary<string, Variant> GetCurrentToolSettings()
    {
        if (_currentTool != null)
        {
            return _currentTool.GetToolSettings();
        }
        return new Dictionary<string, Variant>();
    }
}
