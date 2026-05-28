using Godot;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class ToolManager : Node
{
    public signal ToolChanged(BaseTool tool);

    private Dictionary<string, BaseTool> _tools = new();
    private BaseTool _currentTool;

    public BaseTool CurrentTool => _currentTool;

    public void RegisterTool(BaseTool tool)
    {
        _tools[tool.ToolName] = tool;
        tool.Initialize(GetParent<Main>(), GetNode<LayerManager>("../LayerManager"), GetNode<HistoryManager>("../HistoryManager"));
        
        if (_currentTool == null)
        {
            SetTool(tool);
        }
    }

    public void SetTool(string toolName)
    {
        if (_tools.ContainsKey(toolName))
        {
            SetTool(_tools[toolName]);
        }
    }

    public void SetTool(BaseTool tool)
    {
        if (_currentTool != null)
            _currentTool.OnDeactivate();

        _currentTool = tool;
        _currentTool.OnActivate();
        ToolChanged.Emit(tool);
        GD.Print($"Herramienta activa: {tool.ToolName}");
    }

    public void HandleInput(InputEvent e)
    {
        if (_currentTool != null)
            _currentTool.OnInput(e);
    }
    
    public BaseTool GetToolByName(string name) => _tools.ContainsKey(name) ? _tools[name] : null;
}
