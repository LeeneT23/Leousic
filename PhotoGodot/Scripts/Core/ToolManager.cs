using Godot;
using System.Collections.Generic;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Manages all tools and handles tool switching
    /// </summary>
    public class ToolManager : Node
    {
        [Signal] public delegate void ToolChangedEventHandler(BaseTool newTool);
        [Signal] public delegate void ToolAddedEventHandler(BaseTool tool);
        
        private Dictionary<string, BaseTool> _tools = new Dictionary<string, BaseTool>();
        private BaseTool _currentTool;
        private BaseTool _previousTool;
        
        // Canvas references
        private ImageTexture _canvasTexture;
        private Image _canvasImage;
        private Control _canvasControl;
        
        public BaseTool CurrentTool => _currentTool;
        public BaseTool PreviousTool => _previousTool;
        public IReadOnlyDictionary<string, BaseTool> Tools => _tools.AsReadOnly();
        
        public override void _Ready()
        {
            GD.Print("[ToolManager] Initialized");
        }
        
        /// <summary>
        /// Initialize the tool manager with canvas references
        /// </summary>
        public void Initialize(ImageTexture texture, Image image, Control canvas)
        {
            _canvasTexture = texture;
            _canvasImage = image;
            _canvasControl = canvas;
            
            // Initialize all registered tools
            foreach (var tool in _tools.Values)
            {
                tool.Initialize(texture, image, canvas);
            }
        }
        
        /// <summary>
        /// Register a new tool
        /// </summary>
        public void RegisterTool(BaseTool tool)
        {
            if (tool == null)
            {
                GD.PrintErr("[ToolManager] Cannot register null tool");
                return;
            }
            
            string toolName = tool.ToolName;
            
            if (_tools.ContainsKey(toolName))
            {
                GD.Print($"[ToolManager] Replacing existing tool: {toolName}");
                _tools[toolName].QueueFree();
            }
            
            _tools[toolName] = tool;
            AddChild(tool);
            
            // Initialize if canvas is already set
            if (_canvasTexture != null)
            {
                tool.Initialize(_canvasTexture, _canvasImage, _canvasControl);
            }
            
            EmitSignal(SignalName.ToolAdded, tool);
            GD.Print($"[ToolManager] Registered tool: {toolName}");
        }
        
        /// <summary>
        /// Switch to a tool by name
        /// </summary>
        public bool SwitchToTool(string toolName)
        {
            if (!_tools.ContainsKey(toolName))
            {
                GD.PrintErr($"[ToolManager] Tool not found: {toolName}");
                return false;
            }
            
            BaseTool newTool = _tools[toolName];
            
            if (_currentTool == newTool)
            {
                GD.Print($"[ToolManager] Already using tool: {toolName}");
                return true;
            }
            
            // Deactivate current tool
            if (_currentTool != null)
            {
                _previousTool = _currentTool;
                _currentTool.Deactivate();
            }
            
            // Activate new tool
            _currentTool = newTool;
            _currentTool.Activate();
            
            EmitSignal(SignalName.ToolChanged, _currentTool);
            GD.Print($"[ToolManager] Switched to tool: {toolName}");
            
            return true;
        }
        
        /// <summary>
        /// Switch to a tool by index (for keyboard shortcuts)
        /// </summary>
        public bool SwitchToToolByIndex(int index)
        {
            if (index < 0 || index >= _tools.Count)
            {
                GD.PrintErr($"[ToolManager] Invalid tool index: {index}");
                return false;
            }
            
            int currentIndex = 0;
            foreach (var tool in _tools.Values)
            {
                if (currentIndex == index)
                {
                    return SwitchToTool(tool.ToolName);
                }
                currentIndex++;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get a tool by name
        /// </summary>
        public T GetTool<T>(string toolName) where T : BaseTool
        {
            if (_tools.TryGetValue(toolName, out BaseTool tool))
            {
                return tool as T;
            }
            return null;
        }
        
        /// <summary>
        /// Handle canvas input for the current tool
        /// </summary>
        public void HandleInput(Vector2 screenPosition, Vector2 canvasPosition, InputEvent inputEvent)
        {
            if (_currentTool == null) return;
            
            if (inputEvent is InputEventMouseButton mouseButton)
            {
                if (mouseButton.Pressed)
                {
                    _currentTool.OnPress(screenPosition, canvasPosition);
                }
                else
                {
                    _currentTool.OnRelease(screenPosition, canvasPosition);
                }
            }
            else if (inputEvent is InputEventMouseMotion mouseMotion)
            {
                Vector2 fromScreen = screenPosition - mouseMotion.Relative;
                Vector2 fromCanvas = canvasPosition - mouseMotion.Relative;
                
                _currentTool.OnDrag(fromScreen, screenPosition, fromCanvas, canvasPosition);
            }
        }
        
        /// <summary>
        /// Process all tools
        /// </summary>
        public void ProcessTools(double delta)
        {
            if (_currentTool != null)
            {
                _currentTool.Process(delta);
            }
        }
        
        /// <summary>
        /// Draw preview for the current tool
        /// </summary>
        public void DrawToolPreview(CanvasItem canvasItem, Vector2 position)
        {
            if (_currentTool != null)
            {
                _currentTool.DrawPreview(canvasItem, position);
            }
        }
        
        /// <summary>
        /// Update all tools with new canvas references (e.g., after canvas resize)
        /// </summary>
        public void UpdateCanvasReferences(ImageTexture texture, Image image, Control canvas)
        {
            _canvasTexture = texture;
            _canvasImage = image;
            _canvasControl = canvas;
            
            foreach (var tool in _tools.Values)
            {
                tool.Initialize(texture, image, canvas);
            }
        }
        
        /// <summary>
        /// Set a property on the current tool
        /// </summary>
        public void SetCurrentToolProperty(string propertyName, object value)
        {
            if (_currentTool == null) return;
            
            var propertyInfo = _currentTool.GetType().GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                try
                {
                    object convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(_currentTool, convertedValue);
                    GD.Print($"[ToolManager] Set {_currentTool.ToolName}.{propertyName} = {convertedValue}");
                }
                catch (System.Exception e)
                {
                    GD.PrintErr($"[ToolManager] Failed to set property: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Get available tool names
        /// </summary>
        public List<string> GetAvailableTools()
        {
            return new List<string>(_tools.Keys);
        }
    }
}
