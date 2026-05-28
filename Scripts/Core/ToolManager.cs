using Godot;
using System;
using System.Collections.Generic;

namespace PhotoGodot.Core;

public partial class ToolManager : Node
{
    private readonly Dictionary<string, BaseTool> _tools = new();
    private BaseTool? _currentTool;
    
    public BaseTool? CurrentTool => _currentTool;
    public string? CurrentToolName => _currentTool?.Name;
    
    // Tool properties (shared across tools)
    public Color PrimaryColor { get; set; } = Colors.Black;
    public Color SecondaryColor { get; set; } = Colors.White;
    public float BrushSize { get; set; } = 10f;
    public float Opacity { get; set; } = 1.0f;
    public float Hardness { get; set; } = 1.0f;
    
    public event Action<BaseTool?>? OnToolChanged;
    public event Action? OnToolPropertiesChanged;

    private DrawingCanvas? _canvas;
    private LayerManager? _layerManager;
    private HistoryManager? _historyManager;

    public void Initialize(DrawingCanvas canvas, LayerManager layerManager, HistoryManager historyManager)
    {
        _canvas = canvas;
        _layerManager = layerManager;
        _historyManager = historyManager;
        
        RegisterAllTools();
    }

    private void RegisterAllTools()
    {
        RegisterTool(new Tools.BrushTool());
        RegisterTool(new Tools.EraserTool());
        RegisterTool(new Tools.ColorPickerTool());
        RegisterTool(new Tools.MoveTool());
        RegisterTool(new Tools.SelectTool());
        
        // Select brush by default
        SelectTool("Brush");
    }

    public void RegisterTool(BaseTool tool)
    {
        if (_canvas == null || _layerManager == null || _historyManager == null)
        {
            GD.PrintErr("[ToolManager] Cannot register tool before initialization");
            return;
        }
        
        tool.Initialize(_canvas, _layerManager, _historyManager);
        _tools[tool.Name] = tool;
        
        GD.Print($"[ToolManager] Registered tool: {tool.Name}");
    }

    public bool SelectTool(string toolName)
    {
        if (!_tools.ContainsKey(toolName))
        {
            GD.PrintErr($"[ToolManager] Tool not found: {toolName}");
            return false;
        }
        
        // Deactivate current tool
        _currentTool?.Deactivate();
        
        // Activate new tool
        _currentTool = _tools[toolName];
        _currentTool.Activate();
        
        // Apply current properties
        UpdateCurrentToolProperties();
        
        OnToolChanged?.Invoke(_currentTool);
        GD.Print($"[ToolManager] Selected tool: {toolName}");
        
        return true;
    }

    public bool SelectTool<T>() where T : BaseTool
    {
        var toolType = typeof(T).Name;
        return SelectTool(toolType.Replace("Tool", ""));
    }

    public void SetPrimaryColor(Color color)
    {
        PrimaryColor = color;
        UpdateCurrentToolProperties();
        OnToolPropertiesChanged?.Invoke();
    }

    public void SetSecondaryColor(Color color)
    {
        SecondaryColor = color;
        OnToolPropertiesChanged?.Invoke();
    }

    public void SetBrushSize(float size)
    {
        BrushSize = Mathf.Max(1, size);
        UpdateCurrentToolProperties();
        OnToolPropertiesChanged?.Invoke();
    }

    public void SetOpacity(float opacity)
    {
        Opacity = Mathf.Clamp(opacity, 0, 1);
        UpdateCurrentToolProperties();
        OnToolPropertiesChanged?.Invoke();
    }

    public void SetHardness(float hardness)
    {
        Hardness = Mathf.Clamp(hardness, 0, 1);
        UpdateCurrentToolProperties();
        OnToolPropertiesChanged?.Invoke();
    }

    private void UpdateCurrentToolProperties()
    {
        if (_currentTool != null)
        {
            _currentTool.UpdateProperties(BrushSize, Opacity, Hardness, PrimaryColor);
        }
    }

    public IReadOnlyDictionary<string, BaseTool> GetAllTools() => _tools;

    public BaseTool? GetTool(string name)
    {
        return _tools.TryGetValue(name, out var tool) ? tool : null;
    }

    public void HandleKeyDown(Keycode keycode)
    {
        _currentTool?.OnKeyDown(keycode);
    }

    public void Undo()
    {
        if (_historyManager == null || _layerManager == null) return;
        
        var entry = _historyManager.Undo();
        if (entry != null && entry.LayerData != null)
        {
            var layer = _layerManager.GetLayer(entry.LayerIndex);
            if (layer != null)
            {
                var img = Image.New();
                img.LoadPngFromBuffer(entry.LayerData);
                layer.Image = img;
                layer.UpdateTexture();
                _layerManager.OnLayersChanged?.Invoke();
            }
        }
    }

    public void Redo()
    {
        if (_historyManager == null || _layerManager == null) return;
        
        var entry = _historyManager.Redo();
        if (entry != null && entry.LayerData != null)
        {
            var layer = _layerManager.GetLayer(entry.LayerIndex);
            if (layer != null)
            {
                var img = Image.New();
                img.LoadPngFromBuffer(entry.LayerData);
                layer.Image = img;
                layer.UpdateTexture();
                _layerManager.OnLayersChanged?.Invoke();
            }
        }
    }
}
