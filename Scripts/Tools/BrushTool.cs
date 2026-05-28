using Godot;

namespace PhotoGodot.Tools;

public partial class BrushTool : Core.BaseTool
{
    public BrushTool()
    {
        ToolName = "Brush";
    }

    private float _size = 10f;
    private float _hardness = 1f;
    private float _opacity = 1f;

    public override void OnActivate()
    {
        // Sincronizar con settings globales si existen
        if (MainScene != null)
        {
            _size = MainScene.BrushSize;
            _opacity = MainScene.BrushOpacity;
            _hardness = MainScene.BrushHardness;
        }
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            var canvasPos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            
            if (mb.Pressed)
            {
                OnBeginDraw(canvasPos);
                // Dibujar punto inicial
                DrawStroke(LastPos, canvasPos, true);
            }
            else if (IsDrawing)
            {
                OnEndDraw(canvasPos);
            }
        }
        else if (e is InputEventMouseMotion mm && IsDrawing)
        {
            var canvasPos = MainScene.ScreenToCanvas(mm.GlobalPosition);
            DrawStroke(LastPos, canvasPos);
            LastPos = canvasPos;
        }
    }

    private void DrawStroke(Vector2 from, Vector2 to, bool isPoint = false)
    {
        if (LayerManager.ActiveLayer == null) return;

        float dist = from.DistanceTo(to);
        int steps = isPoint ? 1 : (int)(dist / (_size * 0.5f));
        
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0 : (float)i / steps;
            Vector2 pos = from + (to - from) * t;
            
            // Dibujar círculo de pincel
            int radius = (int)(_size / 2);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    float dx = x;
                    float dy = y;
                    float distFromCenter = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (distFromCenter <= radius)
                    {
                        // Calcular dureza (feathering)
                        float alphaFactor = 1f;
                        if (_hardness < 1f)
                        {
                            float edgeDist = radius - distFromCenter;
                            float softEdgeWidth = radius * (1f - _hardness);
                            if (softEdgeWidth > 0 && edgeDist < softEdgeWidth)
                            {
                                alphaFactor = edgeDist / softEdgeWidth;
                            }
                        }
                        
                        Color brushColor = MainScene.CurrentColor;
                        brushColor.A *= _opacity * alphaFactor;
                        
                        LayerManager.ActiveLayer.DrawPixel(pos + new Vector2(x, y), brushColor);
                    }
                }
            }
        }
        
        CommitChanges();
    }
}
