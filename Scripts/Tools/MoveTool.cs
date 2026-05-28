using Godot;

public partial class MoveTool : BaseTool
{
    private Vector2 _startPosition;
    private bool _isMoving = false;
    
    public MoveTool()
    {
        _toolName = "Move";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        _startPosition = position;
        _isMoving = true;
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        if (!_isMoving) return;
        
        // For now, move tool shows feedback but doesn't actually move pixels
        // A full implementation would require tracking layer offsets
        GD.Print($"Moving: {delta}");
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        _isMoving = false;
    }
    
    public override void HandleInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _startPosition = mouseButton.Position;
                    _isMoving = true;
                    OnPressStart(_startPosition);
                }
                else
                {
                    OnPressEnd(mouseButton.Position);
                    _isMoving = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isMoving)
        {
            Vector2 currentPosition = mouseMotion.Position;
            Vector2 delta = currentPosition - _startPosition;
            OnDraw(_startPosition, currentPosition, delta);
            _startPosition = currentPosition;
        }
    }
}
