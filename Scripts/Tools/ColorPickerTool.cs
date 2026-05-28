using Godot;

/// <summary>
/// Herramienta de selector de color (cuentagotas).
/// Permite tomar colores directamente del canvas.
/// </summary>
public partial class ColorPickerTool : BaseTool
{
    public ColorPickerTool()
    {
        ToolName = "Selector de Color";
        ToolDescription = "Toma un color del canvas (cuentagotas)";
    }
    
    protected override void OnDrawStart(Vector2 position)
    {
        SampleColor(position);
    }
    
    private void SampleColor(Vector2 position)
    {
        if (Canvas == null)
            return;
        
        // Obtener el color del pixel en la posición
        var layers = Canvas.GetLayer(0);
        if (layers != null && layers.Texture != null)
        {
            Image img = layers.Texture.GetImage();
            int x = (int)position.X;
            int y = (int)position.Y;
            
            if (x >= 0 && x < img.GetSize().X && y >= 0 && y < img.GetSize().Y)
            {
                Color sampledColor = img.GetPixel(x, y);
                PrimaryColor = sampledColor;
                
                GD.Print($"[ColorPicker] Color seleccionado: {sampledColor.ToHtml()}");
                
                // Notificar a la UI si existe
                if (UI != null)
                {
                    UI.OnColorPicked(sampledColor);
                }
            }
        }
    }
}
