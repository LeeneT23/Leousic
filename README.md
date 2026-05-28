# 🎨 PhotoGodot Pro v2.0

Editor de imágenes completo estilo Photoshop desarrollado en C# para Godot 4.6.

## ✨ Características

### Herramientas
- **🖌️ Pincel**: Dibujo con control de tamaño, dureza y opacidad
- **🧼 Borrador**: Borrado suave con control de dureza
- **💉 Selector de Color**: Cuentagotas para seleccionar colores del lienzo
- **✋ Mover**: Mover elementos en el lienzo
- **⬜ Selección**: Selección rectangular

### Sistema de Capas
- Crear, eliminar y duplicar capas
- Reordenar capas (arrastrar en la lista)
- Fusionar hacia abajo
- Aplanar todas las capas
- Visibilidad por capa
- Opacidad individual

### Historial
- Undo/Redo ilimitado (configurable, default 50 estados)
- Ctrl+Z para deshacer
- Ctrl+Shift+Z para rehacer

### Vista
- Zoom con Ctrl+Rueda del ratón o Ctrl++/-
- Grid toggle con tecla G
- Paneo con rueda central (implementación básica)

### Exportación
- Guardar como PNG con Ctrl+S
- Soporte para JPG y otros formatos

## 🚀 Cómo Usar

1. Abre Godot 4.6
2. Importa este proyecto
3. Presiona F5 para ejecutar

## ⌨️ Atajos de Teclado

| Acción | Atajo |
|--------|-------|
| Pincel | B |
| Borrador | E |
| Selector | I |
| Mover | V |
| Selección | M |
| Grid | G |
| Undo | Ctrl+Z |
| Redo | Ctrl+Shift+Z |
| Nuevo | Ctrl+N |
| Exportar | Ctrl+S/E |
| Zoom In | Ctrl++ |
| Zoom Out | Ctrl+- |

## 🏗️ Arquitectura

```
Scripts/
├── Main.cs              # Controlador principal
├── Core/
│   ├── Layer.cs         # Clase de capa
│   ├── LayerManager.cs  # Gestor de capas
│   ├── HistoryManager.cs # Undo/Redo
│   ├── BaseTool.cs      # Clase base herramientas
│   └── ToolManager.cs   # Gestor de herramientas
├── Tools/
│   ├── BrushTool.cs     # Pincel
│   ├── EraserTool.cs    # Borrador
│   ├── ColorPickerTool.cs # Selector
│   ├── MoveTool.cs      # Mover
│   └── SelectTool.cs    # Selección
└── UI/
    └── MainUI.cs        # Interfaz completa
```

## 🔧 Crear Herramientas Personalizadas

```csharp
using Godot;
using PhotoGodot.Core;

public partial class MiHerramienta : BaseTool
{
    public override void OnActivate()
    {
        GD.Print("Mi herramienta activada");
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2 pos = MainScene.GetCanvasPosition(mb.Position);
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

- El lienzo es de 1920x1080 píxeles
- La ventana es de 1280x720 (ajustable)
- Todas las herramientas soportan clic izquierdo (color primario) y derecho (secundario)

## 🎯 Estado del Proyecto

✅ Funcionalidades básicas completas
✅ Sistema de capas operativo
✅ Herramientas de dibujo funcionales
✅ UI completa generada por código
✅ Sin dependencia de escenas .tscn complejas

## 📄 Licencia

Proyecto educativo/demostrativo.
