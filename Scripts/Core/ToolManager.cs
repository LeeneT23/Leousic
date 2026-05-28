using Godot;
using System.Collections.Generic;

public partial class ToolManager : Node
{
    private Main _main;
    private Dictionary<string, BaseTool> _tools = new();
    private BaseTool _currentTool;
    
    public BaseTool CurrentTool => _currentTool;
    public string CurrentToolName => _currentTool?.ToolName ?? "None";
    
    public void Initialize(Main main)
    {
        _main = main;
    }
    
    public void RegisterTool(BaseTool tool)
    {
        if (tool == null) return;
        
        tool.Initialize(_main);
        AddChild(tool);
        _tools[tool.ToolName] = tool;
        
        GD.Print($"Tool registered: {tool.ToolName}");
    }
    
    public void SetActiveTool(string toolName)
    {
        if (!_tools.ContainsKey(toolName))
        {
            GD.PrintErr($"Tool not found: {toolName}");
            return;
        }
        
        _currentTool?.OnDeactivate();
        _currentTool = _tools[toolName];
        _currentTool.OnActivate();
        
        GD.Print($"Active tool: {toolName}");
        
        if (_main.GetMainUI() != null)
        {
            _main.GetMainUI().UpdateToolLabel(toolName);
        }
    }
    
    public BaseTool GetTool(string toolName)
    {
        return _tools.TryGetValue(toolName, out var tool) ? tool : null;
    }
    
    public Dictionary<string, BaseTool> GetAllTools() => _tools;
}
