using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Canvas principal de dibujo donde se renderizan las capas y herramientas.
/// Soporta zoom, paneo, grid y múltiples capas.
/// </summary>
public partial class DrawingCanvas : Node2D
{
    [Signal] public delegate void CanvasUpdatedEventHandler();
    [Signal] public delegate void ZoomChangedEventHandler(float zoom);
    [Signal] public delegate void PanChangedEventHandler(Vector2 offset);
    
    // Configuración del canvas
    [Export] public int CanvasWidth { get; set; } = 1920;
    [Export] public int CanvasHeight { get; set; } = 1080;
    [Export] public Color BackgroundColor { get; set; } = Colors.White;
    [Export] public bool ShowGrid { get; set; } = false;
    [Export] public int GridSize { get; set; } = 50;
    [Export] public Color GridColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    
    // Estado del viewport
    public float CurrentZoom { get; private set; } = 1.0f;
    public Vector2 PanOffset { get; private set; } = Vector2.Zero;
    public float MinZoom { get; set; } = 0.1f;
    public float MaxZoom { get; set; } = 10.0f;
    
    // Referencias
    private LayerManager? _layerManager;
    private ToolManager? _toolManager;
    private HistoryManager? _historyManager;
    
    // Cache de renderizado
    private Image? _compositeImage;
    private ImageTexture? _compositeTexture;
    private Sprite2D? _compositeSprite;
    private bool _needsCompositeUpdate = true;
    
    public override void _Ready()
    {
        // Configurar el nodo para recibir input
        SetProcessInput(true);
        
        // Inicializar textura compuesta
        InitializeCompositeTexture();
        
        GD.Print($"[DrawingCanvas] Canvas inicializado: {CanvasWidth}x{CanvasHeight}");
    }
    
    /// <summary>
    /// Inicializa la textura compuesta
    /// </summary>
    private void InitializeCompositeTexture()
    {
        _compositeImage = Image.CreateEmpty(CanvasWidth, CanvasHeight, false, Image.Format.Rgba8);
        _compositeImage.Fill(Colors.Transparent);
        
        _compositeTexture = ImageTexture.CreateFromImage(_compositeImage);
        
        _compositeSprite = new Sprite2D();
        _compositeSprite.Texture = _compositeTexture;
        _compositeSprite.Centered = false;
        AddChild(_compositeSprite);
    }
    
    /// <summary>
    /// Establece las referencias a los managers
    /// </summary>
    public void SetManagers(LayerManager layerManager, ToolManager toolManager, HistoryManager historyManager)
    {
        _layerManager = layerManager;
        _toolManager = toolManager;
        _historyManager = historyManager;
    }
    
    public override void _Draw()
    {
        // Dibujar fondo
        DrawRect(new Rect2(0, 0, CanvasWidth, CanvasHeight), BackgroundColor);
        
        // Dibujar grid si está activado
        if (ShowGrid)
        {
            DrawGrid();
        }
        
        // Dibujar borde del canvas
        DrawRect(new Rect2(0, 0, CanvasWidth, CanvasHeight), Colors.Black, false, 2);
    }
    
    /// <summary>
    /// Dibuja el grid de referencia
    /// </summary>
    private void DrawGrid()
    {
        // Líneas verticales
        for (int x = 0; x <= CanvasWidth; x += GridSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, CanvasHeight), GridColor, 1);
        }
        
        // Líneas horizontales
        for (int y = 0; y <= CanvasHeight; y += GridSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(CanvasWidth, y), GridColor, 1);
        }
    }
    
    /// <summary>
    /// Actualiza la composición de todas las capas visibles
    /// </summary>
    public void UpdateComposite()
    {
        if (_layerManager == null || _compositeImage == null)
            return;
        
        _compositeImage.Lock();
        _compositeImage.Fill(Colors.Transparent);
        
        // Composición de capas en orden
        var layers = _layerManager.GetAllLayers();
        foreach (var layer in layers)
        {
            if (layer.Visible && layer.Texture != null)
            {
                CompositeLayer(layer);
            }
        }
        
        _compositeImage.Unlock();
        _compositeTexture?.Update(_compositeImage);
        
        _needsCompositeUpdate = false;
        EmitSignal(SignalName.CanvasUpdated);
    }
    
    /// <summary>
    /// Compone una capa individual en la imagen compuesta
    /// </summary>
    private void CompositeLayer(Layer layer)
    {
        if (layer.Texture == null || _compositeImage == null)
            return;
        
        Image layerImage = layer.Texture.GetImage();
        
        for (int y = 0; y < layerImage.GetSize().Y; y++)
        {
            for (int x = 0; x < layerImage.GetSize().X; x++)
            {
                int targetX = x + (int)layer.Offset.X;
                int targetY = y + (int)layer.Offset.Y;
                
                if (targetX >= 0 && targetX < CanvasWidth && targetY >= 0 && targetY < CanvasHeight)
                {
                    Color pixel = layerImage.GetPixel(x, y);
                    
                    if (pixel.A > 0)
                    {
                        Color existing = _compositeImage.GetPixel(targetX, targetY);
                        Color blended = ApplyBlendMode(existing, pixel, layer.BlendMode);
                        _compositeImage.SetPixel(targetX, targetY, blended);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Aplica el modo de fusión de la capa
    /// </summary>
    private Color ApplyBlendMode(Color background, Color foreground, Layer.BlendModes blendMode)
    {
        return blendMode switch
        {
            Layer.BlendModes.Normal => BlendNormal(background, foreground),
            Layer.BlendModes.Multiply => BlendMultiply(background, foreground),
            Layer.BlendModes.Screen => BlendScreen(background, foreground),
            Layer.BlendModes.Overlay => BlendOverlay(background, foreground),
            Layer.BlendModes.Darken => BlendDarken(background, foreground),
            Layer.BlendModes.Lighten => BlendLighten(background, foreground),
            _ => BlendNormal(background, foreground)
        };
    }
    
    private Color BlendNormal(Color bg, Color fg)
    {
        float alpha = fg.A;
        return new Color(
            bg.R * (1 - alpha) + fg.R * alpha,
            bg.G * (1 - alpha) + fg.G * alpha,
            bg.B * (1 - alpha) + fg.B * alpha,
            Mathf.Max(bg.A, fg.A)
        );
    }
    
    private Color BlendMultiply(Color bg, Color fg)
    {
        Color result = new Color(bg.R * fg.R, bg.G * fg.G, bg.B * fg.B, fg.A);
        return BlendNormal(bg, result);
    }
    
    private Color BlendScreen(Color bg, Color fg)
    {
        Color result = new Color(
            1 - (1 - bg.R) * (1 - fg.R),
            1 - (1 - bg.G) * (1 - fg.G),
            1 - (1 - bg.B) * (1 - fg.B),
            fg.A
        );
        return BlendNormal(bg, result);
    }
    
    private Color BlendOverlay(Color bg, Color fg)
    {
        Color result = new Color(
            bg.R < 0.5 ? 2 * bg.R * fg.R : 1 - 2 * (1 - bg.R) * (1 - fg.R),
            bg.G < 0.5 ? 2 * bg.G * fg.G : 1 - 2 * (1 - bg.G) * (1 - fg.G),
            bg.B < 0.5 ? 2 * bg.B * fg.B : 1 - 2 * (1 - bg.B) * (1 - fg.B),
            fg.A
        );
        return BlendNormal(bg, result);
    }
    
    private Color BlendDarken(Color bg, Color fg)
    {
        Color result = new Color(
            Mathf.Min(bg.R, fg.R),
            Mathf.Min(bg.G, fg.G),
            Mathf.Min(bg.B, fg.B),
            fg.A
        );
        return BlendNormal(bg, result);
    }
    
    private Color BlendLighten(Color bg, Color fg)
    {
        Color result = new Color(
            Mathf.Max(bg.R, fg.R),
            Mathf.Max(bg.G, fg.G),
            Mathf.Max(bg.B, fg.B),
            fg.A
        );
        return BlendNormal(bg, result);
    }
    
    /// <summary>
    /// Maneja el input del usuario
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        // Solo procesar si el mouse está sobre el canvas
        if (@event is InputEventMouse mouseEvent)
        {
            Vector2 mousePos = GetGlobalTransform().AffineInverse() * mouseEvent.Position;
            
            if (mousePos.X < 0 || mousePos.X > CanvasWidth || 
                mousePos.Y < 0 || mousePos.Y > CanvasHeight)
            {
                return;
            }
        }
        
        // Manejar zoom con rueda del mouse
        if (@event is InputEventMouseButton wheelEvent && wheelEvent.ButtonIndex == MouseButton.WheelUp || wheelEvent.ButtonIndex == MouseButton.WheelDown)
        {
            if (wheelEvent.Pressed && Input.IsKeyPressed(Key.Ctrl))
            {
                float zoomDelta = wheelEvent.ButtonIndex == MouseButton.WheelUp ? 0.1f : -0.1f;
                SetZoom(CurrentZoom + zoomDelta);
                wheelEvent.Handled = true;
            }
        }
        
        // Manejar paneo con rueda central o espacio + arrastre
        if (@event is InputEventMouseButton panButton)
        {
            if (panButton.Pressed && (panButton.ButtonIndex == MouseButton.Middle || 
                (panButton.ButtonIndex == MouseButton.Left && Input.IsKeyPressed(Key.Space))))
            {
                Input.SetCustomMouseCursor(CursorShape.CursorMove);
            }
            else if (!panButton.Pressed && (panButton.ButtonIndex == MouseButton.Middle || 
                panButton.ButtonIndex == MouseButton.Left))
            {
                Input.SetCustomMouseCursor(CursorShape.CursorArrow);
            }
        }
    }
    
    /// <summary>
    /// Establece el zoom del canvas
    /// </summary>
    public void SetZoom(float zoom)
    {
        CurrentZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        UpdateTransform();
        EmitSignal(SignalName.ZoomChanged, CurrentZoom);
        QueueRedraw();
    }
    
    /// <summary>
    /// Ajusta el zoom para que todo el canvas sea visible
    /// </summary>
    public void FitToView()
    {
        Viewport viewport = GetViewport();
        if (viewport == null)
            return;
        
        Vector2 viewportSize = viewport.GetVisibleRect().Size;
        float scaleX = viewportSize.X / CanvasWidth;
        float scaleY = viewportSize.Y / CanvasHeight;
        
        SetZoom(Mathf.Min(scaleX, scaleY) * 0.9f);
        CenterView();
    }
    
    /// <summary>
    /// Centra la vista en el canvas
    /// </summary>
    public void CenterView()
    {
        Viewport viewport = GetViewport();
        if (viewport == null)
            return;
        
        Vector2 viewportSize = viewport.GetVisibleRect().Size;
        PanOffset = (viewportSize - new Vector2(CanvasWidth, CanvasHeight) * CurrentZoom) / 2;
        UpdateTransform();
        EmitSignal(SignalName.PanChanged, PanOffset);
    }
    
    /// <summary>
    /// Actualiza la transformación del canvas
    /// </summary>
    private void UpdateTransform()
    {
        Transform2D transform = new Transform2D();
        transform = transform.Scaled(new Vector2(CurrentZoom, CurrentZoom));
        transform = transform.Translated(PanOffset);
        Transform = transform;
        QueueRedraw();
    }
    
    /// <summary>
    /// Marca una capa como modificada
    /// </summary>
    public void MarkLayerAsModified(int layerId)
    {
        _needsCompositeUpdate = true;
        CallDeferred(nameof(UpdateComposite));
    }
    
    /// <summary>
    /// Obtiene una capa por ID
    /// </summary>
    public Layer? GetLayer(int layerId)
    {
        return _layerManager?.GetLayer(layerId);
    }
    
    /// <summary>
    /// Exporta el canvas como imagen
    /// </summary>
    public Image ExportAsImage()
    {
        if (_compositeImage == null)
            return new Image();
        
        Image exportImage = _compositeImage.Duplicate();
        
        // Aplicar fondo blanco si es necesario
        if (BackgroundColor.A > 0)
        {
            Image withBackground = Image.CreateEmpty(CanvasWidth, CanvasHeight, false, Image.Format.Rgba8);
            withBackground.Fill(BackgroundColor);
            
            for (int y = 0; y < CanvasHeight; y++)
            {
                for (int x = 0; x < CanvasWidth; x++)
                {
                    Color composite = exportImage.GetPixel(x, y);
                    Color bg = withBackground.GetPixel(x, y);
                    Color blended = BlendNormal(bg, composite);
                    withBackground.SetPixel(x, y, blended);
                }
            }
            
            exportImage = withBackground;
        }
        
        return exportImage;
    }
    
    /// <summary>
    /// Limpia todo el contenido del canvas
    /// </summary>
    public void Clear()
    {
        if (_compositeImage != null)
        {
            _compositeImage.Fill(Colors.Transparent);
            _compositeTexture?.Update(_compositeImage);
            QueueRedraw();
        }
    }
}
