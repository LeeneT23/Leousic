using Godot;

public partial class DrawingCanvas : Node2D
{
    private Main _main;
    private bool _showGrid = false;
    private int _gridSize = 50;
    private Color _gridColor = new(0.3f, 0.3f, 0.3f, 0.5f);
    
    public bool ShowGrid => _showGrid;
    public int GridSize => _gridSize;
    
    public void Initialize(Main main)
    {
        _main = main;
    }
    
    public override void _Draw()
    {
        if (_showGrid)
        {
            DrawGrid();
        }
    }
    
    private void DrawGrid()
    {
        var viewportSize = GetViewportRect().Size;
        
        // Vertical lines
        for (int x = 0; x <= viewportSize.X; x += _gridSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, viewportSize.Y), _gridColor, 1.0f);
        }
        
        // Horizontal lines
        for (int y = 0; y <= viewportSize.Y; y += _gridSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(viewportSize.X, y), _gridColor, 1.0f);
        }
    }
    
    public void ToggleGrid()
    {
        _showGrid = !_showGrid;
        QueueRedraw();
        GD.Print($"Grid: {(_showGrid ? "ON" : "OFF")}");
    }
    
    public void SetGridSize(int size)
    {
        _gridSize = Mathf.Clamp(size, 10, 200);
        if (_showGrid) QueueRedraw();
    }
    
    public void SetGridColor(Color color)
    {
        _gridColor = color;
        if (_showGrid) QueueRedraw();
    }
}
