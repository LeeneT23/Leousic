using Godot;

namespace PhotoGodot.Core;

public abstract partial class BaseTool : Resource
{
    [Export] public string ToolName { get; set; } = "Herramienta";
    [Export] public string ShortcutKey { get; set; } = "";
    
    protected Main MainScene { get; private set; } = null!;
    protected LayerManager LayerManager { get; private set; } = null!;
    protected HistoryManager History { get; private set; } = null!;

    protected bool IsDrawing { get; set; } = false;
    protected Vector2 LastPos { get; set; } = Vector2.Zero;

    public virtual void Initialize(Main main, LayerManager lm, HistoryManager hist)
    {
        MainScene = main;
        LayerManager = lm;
        History = hist;
    }

    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }

    public virtual void OnInput(InputEvent e) { }

    public virtual void OnBeginDraw(Vector2 pos)
    {
        IsDrawing = true;
        LastPos = pos;
        History.SaveState($"{ToolName} Inicio");
    }

    public virtual void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Implementar en subclases
    }

    public virtual void OnEndDraw(Vector2 pos)
    {
        IsDrawing = false;
    }
    
    protected void CommitChanges()
    {
        if (LayerManager.ActiveLayer != null)
        {
            LayerManager.NotifyLayerUpdated(LayerManager.ActiveLayer);
        }
    }
}
