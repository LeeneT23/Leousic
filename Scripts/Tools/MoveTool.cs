using Godot;

namespace PhotoGodot.Tools;

public partial class MoveTool : Core.BaseTool
{
    public override string Name => "Move";
    public override string Description => "Move the current layer content";

    private bool _isMoving = false;
    private Vector2 _startPos;
    private Image? _dragImage;
    private int _dragOffsetX;
    private int _dragOffsetY;

    protected override void OnLeftMouseDown(Vector2 position)
    {
        if (WorkingLayer == null) return;
        
        _isMoving = true;
        _startPos = position;
        
        var layerPos = ScreenToLayer(position);
        _dragOffsetX = (int)layerPos.X;
        _dragOffsetY = (int)layerPos.Y;
        
        // Store a copy of the current image for preview
        _dragImage = WorkingLayer.Image.Duplicate() as Image;
        
        SaveState("Move Layer", "Moved layer content");
    }

    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_isMoving || WorkingLayer == null || _dragImage == null) return;
        
        var fromLayer = ScreenToLayer(from);
        var toLayer = ScreenToLayer(to);
        
        int offsetX = (int)(toLayer.X - fromLayer.X);
        int offsetY = (int)(toLayer.Y - fromLayer.Y);
        
        // Clear and redraw with offset
        WorkingLayer.Image.Fill(Colors.Transparent);
        
        for (int y = 0; y < _dragImage.GetHeight(); y++)
        {
            for (int x = 0; x < _dragImage.GetWidth(); x++)
            {
                int newX = x + offsetX;
                int newY = y + offsetY;
                
                if (newX >= 0 && newX < WorkingLayer.Width && 
                    newY >= 0 && newY < WorkingLayer.Height)
                {
                    var pixel = _dragImage.GetPixel(x, y);
                    if (pixel.A > 0)
                    {
                        WorkingLayer.Image.SetPixel(newX, newY, pixel);
                    }
                }
            }
        }
        
        WorkingLayer.UpdateTexture();
    }

    protected override void OnLeftMouseUp(Vector2 position)
    {
        _isMoving = false;
        _dragImage = null;
    }

    public override void OnKeyDown(Keycode keycode)
    {
        if (WorkingLayer == null) return;
        
        int step = Input.IsKeyPressed(Key.Shift) ? 10 : 1;
        var img = WorkingLayer.Image;
        
        switch (keycode)
        {
            case Key.Left:
                ShiftLayer(-step, 0);
                break;
            case Key.Right:
                ShiftLayer(step, 0);
                break;
            case Key.Up:
                ShiftLayer(0, -step);
                break;
            case Key.Down:
                ShiftLayer(0, step);
                break;
        }
    }

    private void ShiftLayer(int dx, int dy)
    {
        if (WorkingLayer == null) return;
        
        SaveState("Move Layer", "Shifted layer");
        
        var original = WorkingLayer.Image.Duplicate() as Image;
        WorkingLayer.Image.Fill(Colors.Transparent);
        
        for (int y = 0; y < original.GetHeight(); y++)
        {
            for (int x = 0; x < original.GetWidth(); x++)
            {
                int newX = x + dx;
                int newY = y + dy;
                
                if (newX >= 0 && newX < WorkingLayer.Width && 
                    newY >= 0 && newY < WorkingLayer.Height)
                {
                    var pixel = original.GetPixel(x, y);
                    WorkingLayer.Image.SetPixel(newX, newY, pixel);
                }
            }
        }
        
        WorkingLayer.UpdateTexture();
    }
}
