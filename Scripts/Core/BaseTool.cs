using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Clase base para todas las herramientas de PhotoGodot Pro.
/// Extiende esta clase para crear tus propias herramientas personalizadas.
/// </summary>
public abstract class BaseTool : Node
{
    [Signal] public delegate void ToolActivatedEventHandler();
    [Signal] public delegate void ToolDeactivatedEventHandler();
    
    public string ToolName { get; protected set; } = "Base Tool";
    public string ToolDescription { get; protected set; } = "Herramienta base";
    public Texture2D? ToolIcon { get; protected set; }
    
    protected DrawingCanvas? Canvas { get; private set; }
    protected MainUI? UI { get; private set; }
    
    // Propiedades configurables de la herramienta
    public float BrushSize { get; set; } = 10.0f;
    public float BrushHardness { get; set; } = 1.0f;
    public float Opacity { get; set; } = 1.0f;
    public Color PrimaryColor { get; set; } = Colors.Black;
    public Color SecondaryColor { get; set; } = Colors.White;
    
    // Estado de la herramienta
    protected bool IsDrawing { get; set; } = false;
    protected Vector2 LastPosition { get; set; } = Vector2.Zero;
    
    public virtual void Initialize(DrawingCanvas canvas, MainUI ui)
    {
        Canvas = canvas;
        UI = ui;
    }
    
    /// <summary>
    /// Se llama cuando la herramienta es activada
    /// </summary>
    public virtual void OnActivate()
    {
        EmitSignal(SignalName.ToolActivated);
        GD.Print($"[Tool] {ToolName} activada");
    }
    
    /// <summary>
    /// Se llama cuando la herramienta es desactivada
    /// </summary>
    public virtual void OnDeactivate()
    {
        IsDrawing = false;
        EmitSignal(SignalName.ToolDeactivated);
        GD.Print($"[Tool] {ToolName} desactivada");
    }
    
    /// <summary>
    /// Maneja el evento de presión del botón del mouse
    /// </summary>
    public virtual void OnInputPressed(Vector2 position, int buttonIndex, bool shiftPressed, bool ctrlPressed, bool altPressed)
    {
        if (buttonIndex == MouseButton.Left)
        {
            IsDrawing = true;
            LastPosition = position;
            OnDrawStart(position);
        }
        else if (buttonIndex == MouseButton.Right)
        {
            OnRightClick(position);
        }
    }
    
    /// <summary>
    /// Maneja el evento de movimiento del mouse con botón presionado
    /// </summary>
    public virtual void OnInputDragged(Vector2 position, Vector2 delta, int buttonMask)
    {
        if (IsDrawing && (buttonMask & MouseButtonMask.Left) != 0)
        {
            OnDraw(LastPosition, position, delta);
            LastPosition = position;
        }
    }
    
    /// <summary>
    /// Maneja el evento de liberación del botón del mouse
    /// </summary>
    public virtual void OnInputReleased(Vector2 position, int buttonIndex)
    {
        if (buttonIndex == MouseButton.Left && IsDrawing)
        {
            OnDrawEnd(position);
            IsDrawing = false;
        }
    }
    
    /// <summary>
    /// Se llama al iniciar un trazo
    /// </summary>
    protected virtual void OnDrawStart(Vector2 position) { }
    
    /// <summary>
    /// Se llama durante el trazo (para dibujar continuamente)
    /// </summary>
    protected virtual void OnDraw(Vector2 from, Vector2 to, Vector2 delta) { }
    
    /// <summary>
    /// Se llama al finalizar un trazo
    /// </summary>
    protected virtual void OnDrawEnd(Vector2 position) { }
    
    /// <summary>
    /// Se llama al hacer clic derecho
    /// </summary>
    protected virtual void OnRightClick(Vector2 position) { }
    
    /// <summary>
    /// Se llama al procesar input general (teclado, etc.)
    /// </summary>
    public virtual void ProcessInput(InputEvent @event) { }
    
    /// <summary>
    /// Obtiene los ajustes personalizados para esta herramienta
    /// </summary>
    public virtual Dictionary<string, Variant> GetToolSettings()
    {
        return new Dictionary<string, Variant>
        {
            { "BrushSize", BrushSize },
            { "BrushHardness", BrushHardness },
            { "Opacity", Opacity }
        };
    }
    
    /// <summary>
    /// Aplica los ajustes desde la UI
    /// </summary>
    public virtual void ApplyToolSettings(Dictionary<string, Variant> settings)
    {
        if (settings.TryGetValue("BrushSize", out var size))
            BrushSize = size.AsSingle();
        if (settings.TryGetValue("BrushHardness", out var hardness))
            BrushHardness = hardness.AsSingle();
        if (settings.TryGetValue("Opacity", out var opacity))
            Opacity = opacity.AsSingle();
    }
}
