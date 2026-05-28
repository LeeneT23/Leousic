using Godot;

namespace PhotoGodot.Tools;

public partial class ColorPickerTool : Core.BaseTool
{
    public ColorPickerTool()
    {
        ToolName = "ColorPicker";
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            var canvasPos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            
            if (LayerManager.ActiveLayer != null)
            {
                Color pickedColor = LayerManager.ActiveLayer.GetPixel(canvasPos);
                MainScene.CurrentColor = pickedColor;
                GD.Print($"Color seleccionado: {pickedColor.ToHtml()}");
            }
        }
    }
}
