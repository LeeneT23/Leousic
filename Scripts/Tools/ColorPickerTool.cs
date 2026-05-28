using Godot;

public partial class ColorPickerTool : BaseTool
{
    public ColorPickerTool()
    {
        _toolName = "ColorPicker";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        PickColor(position);
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Color picker only picks on click, not while dragging
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        // Do nothing on release
    }
    
    private void PickColor(Vector2 position)
    {
        var compositedImage = _main.GetLayerManager().GetCompositedImage();
        if (compositedImage == null) return;
        
        int x = (int)position.X;
        int y = (int)position.Y;
        
        if (x >= 0 && x < compositedImage.GetWidth() && 
            y >= 0 && y < compositedImage.GetHeight())
        {
            Color pickedColor = compositedImage.GetPixel(x, y);
            _main.SetPrimaryColor(pickedColor);
            
            GD.Print($"Color picked: {pickedColor.ToHtml()}");
            
            if (_main.GetMainUI() != null)
            {
                _main.GetMainUI().UpdateColorPicker(pickedColor);
            }
        }
    }
}
