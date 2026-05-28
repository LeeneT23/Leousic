using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class ColorPickerTool : BaseTool
{
    public override void OnActivate()
    {
        GD.Print("💉 Selector de color activado");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
            PickColor(pos);
        }
    }

    private void PickColor(Vector2 pos)
    {
        if (LayerManager.ActiveLayer == null) return;
        
        Color picked = LayerManager.ActiveLayer.GetPixel(pos);
        
        if (picked.A > 0)
        {
            MainScene.PrimaryColor = picked;
            GD.Print($"🎨 Color seleccionado: {picked.ToHtml()}");
        }
    }
}
