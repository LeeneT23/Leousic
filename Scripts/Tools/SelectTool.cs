using Godot;

public partial class SelectTool : BaseTool
{
    private Vector2 _selectionStart;
    private Rect2 _selectionRect;
    private bool _isSelecting = false;
    
    public Rect2 SelectionRect => _selectionRect;
    public bool HasSelection => _selectionRect.Size.X > 0 && _selectionRect.Size.Y > 0;
    
    public SelectTool()
    {
        _toolName = "Select";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        _selectionStart = position;
        _selectionRect = new Rect2(position, Vector2.Zero);
        _isSelecting = true;
        
        GD.Print($"Selection started at: {position}");
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_isSelecting) return;
        
        // Update selection rectangle
        Vector2 size = to - _selectionStart;
        _selectionRect = new Rect2(
            _selectionStart.X < to.X ? _selectionStart.X : to.X,
            _selectionStart.Y < to.Y ? _selectionStart.Y : to.Y,
            Mathf.Abs(size.X),
            Mathf.Abs(size.Y)
        );
        
        GD.Print($"Selecting: {_selectionRect}");
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        _isSelecting = false;
        
        // Ensure minimum selection size
        if (_selectionRect.Size.X < 1 || _selectionRect.Size.Y < 1)
        {
            _selectionRect = new Rect2();
        }
        
        if (HasSelection)
        {
            GD.Print($"Selection complete: {_selectionRect}");
        }
    }
    
    public void ClearSelection()
    {
        _selectionRect = new Rect2();
        GD.Print("Selection cleared");
    }
    
    public void CopySelection()
    {
        if (!HasSelection) return;
        
        var activeLayer = _main.GetLayerManager().ActiveLayer;
        if (activeLayer == null) return;
        
        var image = activeLayer.GetImage();
        int x = (int)_selectionRect.Position.X;
        int y = (int)_selectionRect.Position.Y;
        int width = (int)_selectionRect.Size.X;
        int height = (int)_selectionRect.Size.Y;
        
        // Clamp to image bounds
        x = Mathf.Clamp(x, 0, image.GetWidth());
        y = Mathf.Clamp(y, 0, image.GetHeight());
        width = Mathf.Min(width, image.GetWidth() - x);
        height = Mathf.Min(height, image.GetHeight() - y);
        
        if (width > 0 && height > 0)
        {
            var croppedImage = image.GetRegion(new Rect2i(x, y, width, height));
            GD.Print($"Copied selection: {width}x{height}");
            // Store in clipboard (would need a clipboard manager for full implementation)
        }
    }
    
    public void DeleteSelection()
    {
        if (!HasSelection) return;
        
        var activeLayer = _main.GetLayerManager().ActiveLayer;
        if (activeLayer == null) return;
        
        var image = activeLayer.GetImage();
        int x = (int)_selectionRect.Position.X;
        int y = (int)_selectionRect.Position.Y;
        int width = (int)_selectionRect.Size.X;
        int height = (int)_selectionRect.Size.Y;
        
        for (int py = y; py < y + height && py < image.GetHeight(); py++)
        {
            for (int px = x; px < x + width && px < image.GetWidth(); px++)
            {
                if (px >= 0 && py >= 0)
                {
                    Color pixelColor = image.GetPixel(px, py);
                    Color transparentColor = new(pixelColor.R, pixelColor.G, pixelColor.B, 0);
                    image.SetPixel(px, py, transparentColor);
                }
            }
        }
        
        GD.Print("Selection deleted");
        ClearSelection();
    }
}
