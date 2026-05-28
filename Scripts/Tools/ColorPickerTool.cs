using Godot;

namespace PhotoGodot.Tools;

public partial class ColorPickerTool : Core.BaseTool
{
    public override string Name => "ColorPicker";
    public override string Description => "Pick color from canvas (Eyedropper)";

    protected override void OnLeftMouseDown(Vector2 position)
    {
        PickColor(position);
    }

    private void PickColor(Vector2 position)
    {
        if (LayerManager == null || LayerManager.ActiveLayer == null) return;
        
        var layerPos = ScreenToLayer(position);
        int x = (int)layerPos.X;
        int y = (int)layerPos.Y;
        
        var layer = LayerManager.ActiveLayer;
        if (x >= 0 && x < layer.Width && y >= 0 && y < layer.Height)
        {
            var color = layer.Image.GetPixel(x, y);
            
            // Set as primary color
            if (ToolManager != null)
            {
                ToolManager.SetPrimaryColor(color);
            }
            
            GD.Print($"[ColorPicker] Picked: {color.ToHtml()}");
        }
    }
}
