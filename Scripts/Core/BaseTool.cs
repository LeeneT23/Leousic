using Godot;

public abstract partial class BaseTool : Node
{
    protected Main _main;
    protected string _toolName;
    protected bool _isDrawing = false;
    protected Vector2 _lastPosition;
    
    public string ToolName => _toolName;
    public bool IsDrawing => _isDrawing;
    
    public virtual void Initialize(Main main)
    {
        _main = main;
    }
    
    public virtual void HandleInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDrawing = true;
                    _lastPosition = GetCanvasPosition(mouseButton.Position);
                    OnPressStart(_lastPosition);
                }
                else
                {
                    OnPressEnd(_lastPosition);
                    _isDrawing = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isDrawing)
        {
            Vector2 currentPosition = GetCanvasPosition(mouseMotion.Position);
            OnDraw(_lastPosition, currentPosition, currentPosition - _lastPosition);
            _lastPosition = currentPosition;
        }
    }
    
    protected virtual Vector2 GetCanvasPosition(Vector2 screenPosition)
    {
        return screenPosition;
    }
    
    protected abstract void OnPressStart(Vector2 position);
    protected abstract void OnDraw(Vector2 from, Vector2 to, Vector2 delta);
    protected abstract void OnPressEnd(Vector2 position);
    
    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
}
