using Godot;

namespace PhotoGodot.Tools;

public partial class SelectTool : Core.BaseTool
{
    public override string Name => "Select";
    public override string Description => "Make rectangular selections";

    public bool HasSelection { get; private set; }
    public Rect2 SelectionRect { get; private set; }
    
    private bool _isSelecting = false;
    private Vector2 _startPos;

    protected override void OnLeftMouseDown(Vector2 position)
    {
        _isSelecting = true;
        _startPos = position;
        
        var layerPos = ScreenToLayer(position);
        SelectionRect = new Rect2(layerPos.X, layerPos.Y, 0, 0);
    }

    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_isSelecting) return;
        
        var fromLayer = ScreenToLayer(from);
        var toLayer = ScreenToLayer(to);
        
        float x = Mathf.Min(fromLayer.X, toLayer.X);
        float y = Mathf.Min(fromLayer.Y, toLayer.Y);
        float width = Mathf.Abs(toLayer.X - fromLayer.X);
        float height = Mathf.Abs(toLayer.Y - fromLayer.Y);
        
        SelectionRect = new Rect2(x, y, width, height);
        HasSelection = width > 1 && height > 1;
        
        // Request canvas redraw to show selection
        if (Canvas != null)
        {
            Canvas.QueueRedraw();
        }
    }

    protected override void OnLeftMouseUp(Vector2 position)
    {
        _isSelecting = false;
        
        if (HasSelection)
        {
            SaveState("Selection", "Created selection");
            GD.Print($"[SelectTool] Selection: {SelectionRect}");
        }
    }

    public void ClearSelection()
    {
        HasSelection = false;
        SelectionRect = new Rect2(0, 0, 0, 0);
        Canvas?.QueueRedraw();
    }

    public void CopySelection()
    {
        if (!HasSelection || WorkingLayer == null) return;
        
        int x = (int)SelectionRect.Position.X;
        int y = (int)SelectionRect.Position.Y;
        int w = (int)SelectionRect.Size.X;
        int h = (int)SelectionRect.Size.Y;
        
        if (w <= 0 || h <= 0) return;
        
        var cropped = WorkingLayer.Image.GetRegion(new Rect2I(x, y, w, h));
        GD.Print($"[SelectTool] Copied region: {w}x{h}");
        
        // Could store for paste operation
    }

    public void CutSelection()
    {
        if (!HasSelection || WorkingLayer == null) return;
        
        CopySelection();
        
        // Clear the selected area
        for (int y = (int)SelectionRect.Position.Y; 
             y < SelectionRect.End.Y && y < WorkingLayer.Height; y++)
        {
            for (int x = (int)SelectionRect.Position.X; 
                 x < SelectionRect.End.X && x < WorkingLayer.Width; x++)
            {
                if (x >= 0 && y >= 0)
                {
                    WorkingLayer.Image.SetPixel(x, y, Colors.Transparent);
                }
            }
        }
        
        WorkingLayer.UpdateTexture();
        ClearSelection();
    }

    public void FillSelection(Color color)
    {
        if (!HasSelection || WorkingLayer == null) return;
        
        SaveState("Fill Selection", "Filled selection with color");
        
        WorkingLayer.FillRect(SelectionRect, color, Opacity);
        WorkingLayer.UpdateTexture();
    }

    public void DeleteSelection()
    {
        if (!HasSelection || WorkingLayer == null) return;
        
        SaveState("Delete Selection", "Deleted selected area");
        
        for (int y = (int)SelectionRect.Position.Y; 
             y < SelectionRect.End.Y && y < WorkingLayer.Height; y++)
        {
            for (int x = (int)SelectionRect.Position.X; 
                 x < SelectionRect.End.X && x < WorkingLayer.Width; x++)
            {
                if (x >= 0 && y >= 0)
                {
                    WorkingLayer.Image.SetPixel(x, y, Colors.Transparent);
                }
            }
        }
        
        WorkingLayer.UpdateTexture();
        ClearSelection();
    }

    public override void OnKeyDown(Keycode keycode)
    {
        switch (keycode)
        {
            case Key.Delete:
            case Key.BackSpace:
                DeleteSelection();
                break;
            case Key.C:
                if (Input.IsKeyPressed(Key.Ctrl))
                {
                    CopySelection();
                }
                break;
            case Key.X:
                if (Input.IsKeyPressed(Key.Ctrl))
                {
                    CutSelection();
                }
                break;
            case Key.Escape:
                ClearSelection();
                break;
        }
    }
}
