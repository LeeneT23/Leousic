# PhotoGodot - Advanced Image Editor for Godot 4.3

A powerful, extensible image editing application built with C# and Godot 4.3, featuring Photoshop-like functionality.

## Features

### Core Tools
- **Brush Tool** - Paint with customizable size, opacity, hardness, and shape (Circle, Square, Soft)
- **Eraser Tool** - Remove pixels with transparent or background color mode
- **Color Picker** - Sample colors from canvas (single pixel or area average)
- **Move Tool** - Pan and navigate around the canvas
- **Select Tool** - Create rectangular selections

### Layer System
- Multiple layers with full compositing
- Layer visibility toggle
- Opacity control per layer
- Blend modes: Normal, Multiply, Screen, Darken, Lighten
- Layer reordering
- Duplicate layers
- Merge visible layers

### History & Undo
- Full undo/redo support (configurable history size, default 50 states)
- Non-destructive editing

### Canvas Features
- Zoom in/out (0.1x to 10x)
- Pan navigation
- Grid overlay toggle
- Checkerboard transparency preview
- Canvas resize with content preservation

### UI Features
- Menu bar with File, Edit, View, Help menus
- Toolbar with quick tool access
- Tool options panel (color picker, brush size, opacity)
- Status bar with real-time information

### Keyboard Shortcuts
| Key | Action |
|-----|--------|
| 1 | Select Tool |
| 2 | Brush Tool |
| 3 | Eraser Tool |
| 4 | Move Tool |
| 5 | Color Picker |
| Ctrl+S | Save Project |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+G | Toggle Grid |
| Ctrl++ | Zoom In |
| Ctrl+- | Zoom Out |
| Ctrl+0 | Reset Zoom |
| Mouse Wheel | Zoom |
| Escape | Quit |

## Project Structure

```
PhotoGodot/
├── project.godot          # Godot project configuration
├── icon.svg               # Application icon
├── Scenes/
│   └── Main.tscn          # Main scene file
├── Scripts/
│   ├── Main.cs            # Main entry point
│   ├── Core/
│   │   ├── BaseTool.cs       # Base class for all tools
│   │   ├── ToolManager.cs    # Tool management system
│   │   ├── HistoryManager.cs # Undo/redo system
│   │   ├── DrawingCanvas.cs  # Canvas component
│   │   ├── Layer.cs          # Single layer class
│   │   └── LayerManager.cs   # Layer stack management
│   ├── Tools/
│   │   ├── BrushTool.cs      # Brush implementation
│   │   ├── EraserTool.cs     # Eraser implementation
│   │   ├── ColorPickerTool.cs# Color picker
│   │   ├── MoveTool.cs       # Move/navigation
│   │   └── SelectTool.cs     # Selection tool
│   └── UI/
│       └── MainUI.cs         # User interface controller
└── Assets/
    └── Icons/                # Tool icons (extensible)
```

## How to Use

1. Open the project in Godot 4.3
2. Run the Main scene (Scenes/Main.tscn)
3. Select a tool using number keys (1-5) or toolbar buttons
4. Start drawing on the canvas!

## Extending the Application

### Adding Custom Tools

Create a new class that inherits from `BaseTool`:

```csharp
using Godot;
using PhotoGodot.Core;

public class MyCustomTool : BaseTool
{
    public override string ToolName => "My Tool";
    public override string ToolDescription => "Custom tool description";
    public override string ShortcutKey => "6";
    
    public override void OnPress(Vector2 position, Vector2 canvasPosition)
    {
        // Handle mouse press
    }
    
    public override void OnDrag(Vector2 from, Vector2 to, Vector2 canvasFrom, Vector2 canvasTo)
    {
        // Handle drag
    }
}
```

Register your tool in `Main.cs`:
```csharp
_toolManager.RegisterTool(new MyCustomTool());
```

### Adding New Blend Modes

Extend the `Layer.BlendMode` enum and implement the blending formula in `ApplyBlendMode()`.

## Requirements

- Godot 4.3 or later
- .NET 6+ SDK

## License

This project is provided as-is for educational and commercial use.

---

**Created with ❤️ using Godot 4.3 and C#**
