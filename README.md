# 🎨 PhotoGodot Pro v2.0

Editor de imágenes profesional estilo Photoshop creado en C# para Godot 4.6.

## ✨ Características

### Herramientas
- **Pincel (B)**: Dibujo con tamaño, dureza y opacidad ajustables
- **Borrador (E)**: Borra contenido con tamaño ajustable
- **Selector de Color (I)**: Cuentagotas para seleccionar colores del canvas
- **Mover (V)**: Mueve elementos (funcionalidad básica)
- **Selección (M)**: Selección rectangular

### Sistema de Capas
- Capas ilimitadas
- Visibilidad por capa
- Opacidad individual
- Reordenar capas (arrastrar en la lista)
- Duplicar capas
- Fusionar hacia abajo
- Aplanar todas las capas

### Historial
- Undo/Redo (Ctrl+Z / Ctrl+Shift+Z)
- Hasta 500 estados guardados

### Vista
- Zoom (+/- botones o rueda del ratón)
- Grid toggle (G)
- Canvas de 1920x1080

### Exportación
- PNG (Ctrl+E)
- Guardado de proyecto (Ctrl+S)

## 🚀 Cómo Usar

1. Abre Godot 4.6
2. Importa este proyecto
3. Presiona F5 para ejecutar
4. ¡Comienza a crear!

## ⌨️ Atajos de Teclado

| Tecla | Acción |
|-------|--------|
| B | Pincel |
| E | Borrador |
| I | Selector de color |
| V | Mover |
| M | Selección |
| G | Toggle Grid |
| Ctrl+Z | Deshacer |
| Ctrl+Shift+Z | Rehacer |
| Ctrl+N | Nuevo proyecto |
| Ctrl+S | Guardar |
| Ctrl+E | Exportar PNG |

## 🏗️ Arquitectura

```
Scripts/
├── Main.cs              # Controlador principal
├── Core/
│   ├── Layer.cs         # Clase de capa
│   ├── LayerManager.cs  # Gestor de capas
│   ├── HistoryManager.cs# Sistema undo/redo
│   ├── BaseTool.cs      # Clase base herramientas
│   └── ToolManager.cs   # Gestor de herramientas
├── Tools/
│   ├── BrushTool.cs
│   ├── EraserTool.cs
│   ├── ColorPickerTool.cs
│   ├── MoveTool.cs
│   └── SelectTool.cs
└── UI/
    └── MainUI.cs        # Interfaz completa
```

## 🔧 Crear Herramientas Personalizadas

```csharp
using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class MiHerramienta : BaseTool
{
    public MiHerramienta()
    {
        ToolName = "MiHerramienta";
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed)
        {
            var pos = MainScene.ScreenToCanvas(mb.GlobalPosition);
            // Tu lógica aquí
        }
    }
}
```

Registrar en `Main.cs`:
```csharp
_toolManager.RegisterTool(new MiHerramienta());
```

## 📝 Notas

- Proyecto 100% código C#, sin dependencias de escenas .tscn complejas
- Compatible con Godot 4.6 (también 4.3+)
- Resolución de ventana: 1280x720 (ajustable)
- Tamaño de lienzo: 1920x1080

¡Disfruta creando! 🎨
