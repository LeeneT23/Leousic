using Godot;
using System.Collections.Generic;

/// <summary>
/// Herramienta de selección rectangular.
/// Permite seleccionar áreas del canvas para operaciones posteriores.
/// </summary>
public partial class SelectTool : BaseTool
{
    private Vector2 _selectionStart = Vector2.Zero;
    private Rect2? _currentSelection;
    
    public SelectTool()
    {
        ToolName = "Selección";
        ToolDescription = "Selecciona un área rectangular del canvas";
    }
    
    protected override void OnDrawStart(Vector2 position)
    {
        _selectionStart = position;
        _currentSelection = null;
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Crear rectángulo de selección
        Vector2 size = to - _selectionStart;
        _currentSelection = new Rect2(_selectionStart, size);
        
        // La selección se muestra visualmente en el canvas
        if (Canvas != null)
        {
            Canvas.QueueRedraw();
        }
    }
    
    protected override void OnDrawEnd(Vector2 position)
    {
        if (_currentSelection.HasValue)
        {
            GD.Print($"[SelectTool] Selección creada: {_currentSelection.Value}");
        }
    }
    
    /// <summary>
    /// Obtiene el rectángulo de selección actual
    /// </summary>
    public Rect2? GetSelection()
    {
        return _currentSelection;
    }
    
    /// <summary>
    /// Limpia la selección actual
    /// </summary>
    public void ClearSelection()
    {
        _currentSelection = null;
        if (Canvas != null)
        {
            Canvas.QueueRedraw();
        }
    }
    
    /// <summary>
    /// Corta el contenido seleccionado a una nueva capa
    /// </summary>
    public void CutSelection()
    {
        if (!_currentSelection.HasValue || Canvas == null)
            return;
        
        var activeLayer = Canvas.GetLayer(0);
        if (activeLayer == null || activeLayer.Texture == null)
            return;
        
        Image sourceImage = activeLayer.Texture.GetImage();
        Rect2 selection = _currentSelection.Value;
        
        // Normalizar rectángulo (asegurar que tenga tamaño positivo)
        selection = selection.Abs();
        
        int width = (int)selection.Size.X;
        int height = (int)selection.Size.Y;
        
        if (width <= 0 || height <= 0)
            return;
        
        // Crear nueva imagen con el contenido seleccionado
        Image cutImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcX = (int)(selection.Position.X + x);
                int srcY = (int)(selection.Position.Y + y);
                
                if (srcX >= 0 && srcX < sourceImage.GetSize().X && 
                    srcY >= 0 && srcY < sourceImage.GetSize().Y)
                {
                    cutImage.SetPixel(x, y, sourceImage.GetPixel(srcX, srcY));
                    
                    // Hacer transparente el original
                    sourceImage.SetPixel(srcX, srcY, Colors.Transparent);
                }
            }
        }
        
        // Actualizar capa original
        activeLayer.Texture.Update(sourceImage);
        Canvas.MarkLayerAsModified(activeLayer.Id);
        
        // Crear nueva capa con el contenido cortado
        var newLayer = Layer.CreateFromImage(-1, "Selección", cutImage);
        newLayer.Offset = selection.Position;
        
        GD.Print("[SelectTool] Contenido cortado a nueva capa");
    }
    
    /// <summary>
    /// Copia el contenido seleccionado
    /// </summary>
    public Image? CopySelection()
    {
        if (!_currentSelection.HasValue || Canvas == null)
            return null;
        
        var activeLayer = Canvas.GetLayer(0);
        if (activeLayer == null || activeLayer.Texture == null)
            return null;
        
        Image sourceImage = activeLayer.Texture.GetImage();
        Rect2 selection = _currentSelection.Value.Abs();
        
        int width = (int)selection.Size.X;
        int height = (int)selection.Size.Y;
        
        if (width <= 0 || height <= 0)
            return null;
        
        Image copyImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcX = (int)(selection.Position.X + x);
                int srcY = (int)(selection.Position.Y + y);
                
                if (srcX >= 0 && srcX < sourceImage.GetSize().X && 
                    srcY >= 0 && srcY < sourceImage.GetSize().Y)
                {
                    copyImage.SetPixel(x, y, sourceImage.GetPixel(srcX, srcY));
                }
            }
        }
        
        GD.Print("[SelectTool] Contenido copiado");
        return copyImage;
    }
    
    /// <summary>
    /// Rellena la selección con un color
    /// </summary>
    public void FillSelection(Color color)
    {
        if (!_currentSelection.HasValue || Canvas == null)
            return;
        
        var activeLayer = Canvas.GetLayer(0);
        if (activeLayer == null || activeLayer.Texture == null)
            return;
        
        Image img = activeLayer.Texture.GetImage();
        Rect2 selection = _currentSelection.Value.Abs();
        
        int startX = Mathf.Max(0, (int)selection.Position.X);
        int startY = Mathf.Max(0, (int)selection.Position.Y);
        int endX = Mathf.Min(img.GetSize().X, (int)(selection.Position.X + selection.Size.X));
        int endY = Mathf.Min(img.GetSize().Y, (int)(selection.Position.Y + selection.Size.Y));
        
        img.Lock();
        
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                img.SetPixel(x, y, color);
            }
        }
        
        img.Unlock();
        activeLayer.Texture.Update(img);
        Canvas.MarkLayerAsModified(activeLayer.Id);
        
        GD.Print($"[SelectTool] Selección rellenada con color {color.ToHtml()}");
    }
}
