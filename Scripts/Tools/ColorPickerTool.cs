using Godot;

namespace PhotoGodot.Tools;

public partial class ColorPickerTool : BaseTool
{
    public ColorPickerTool()
    {
        ToolName = "Selector";
        ShortcutKey = "i";
    }

    public override void OnActivate()
    {
        MainScene.SetCursor("cross");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            PickColor(pos);
        }
    }

    private void PickColor(Vector2 pos)
    {
        if (LayerManager.ActiveLayer == null) return;
        
        Color picked = LayerManager.ActiveLayer.GetPixel(pos);
        MainScene.SetCurrentColor(picked);
    }
}
