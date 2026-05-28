# PhotoGodot Pro - Editor de Imágenes Profesional

## 🎨 Descripción
PhotoGodot Pro es un editor de imágenes completo estilo Photoshop, desarrollado en **C#** para **Godot 4.6**. Ofrece herramientas profesionales de edición, sistema de capas, filtros y una arquitectura extensible.

## ✨ Características Principales

### Herramientas de Dibujo
- **🖌 Pincel (B)**: Dibuja con control de tamaño, opacidad y dureza
- **⌫ Borrador (E)**: Elimina píxeles con efecto suave configurable
- **💉 Selector de Color (I)**: Cuentagotas para muestrear colores del canvas
- **✥ Mover (V)**: Desplaza el contenido de la capa actual
- **⛶ Selección (M)**: Selecciona áreas rectangulares

### Sistema de Capas
- ✅ Capas ilimitadas (100 por defecto)
- 👁 Visibilidad por capa
- 🎚 Opacidad ajustable (0-100%)
- 🔄 6 modos de fusión: Mix, Add, Subtract, Multiply, Screen, Overlay
- ⬆️⬇️ Reordenar capas
- 📋 Duplicar capas
- 🔽 Fusionar hacia abajo
- 🧹 Aplanar imagen

### Filtros y Efectos
- ⚪ Escala de grises
- 🔃 Invertir colores
- 💫 Difuminar (Blur)
- 🔍 Enfocar (Sharpen)
- ☀️ Brillo
- 🌓 Contraste

### Vista y Navegación
- 🔍 Zoom: 10% - 1000%
- 📐 Grid/malla configurable
- 🖐 Paneo con rueda del mouse o Space+Click
- 🪟 Ajustar a ventana

### Historial
- ↩️ Undo/Deshacer ilimitado (configurable, default 100)
- ↪️ Redo/Rehacer
- 💾 Guardado automático de estados

### Exportación
- 📄 PNG (sin pérdida)
- 📄 JPG (calidad configurable)
- 📄 WebP (moderno y eficiente)
- 💾 Proyecto .pgd (formato nativo)

## 🎮 Atajos de Teclado

| Acción | Atajo |
|--------|-------|
| Pincel | `B` |
| Borrador | `E` |
| Selector Color | `I` |
| Mover | `V` |
| Selección | `M` |
| Grid | `G` |
| Deshacer | `Ctrl + Z` |
| Rehacer | `Ctrl + Y` o `Ctrl + Shift + Z` |
| Nuevo Proyecto | `Ctrl + N` |
| Guardar | `Ctrl + S` |
| Zoom In | `Ctrl +` |
| Zoom Out | `Ctrl -` |

## 🚀 Instalación y Uso

### Requisitos
- Godot 4.6 o superior
- .NET 6+ (para C#)

### Pasos
1. Abre Godot 4.6
2. Importa el proyecto desde la carpeta `/workspace`
3. Presiona `F5` para ejecutar

### Primeros Pasos
1. Selecciona una herramienta del toolbar superior
2. Elige un color desde el selector
3. Ajusta tamaño, opacidad y dureza según necesites
4. Dibuja en el canvas central
5. Gestiona capas desde el panel derecho
6. Aplica filtros según necesites
7. Exporta tu trabajo cuando termines

## 🏗️ Arquitectura del Proyecto

```
/workspace/
├── project.godot              # Configuración del proyecto
├── icon.svg                   # Icono de la aplicación
├── README.md                  # Este archivo
├── Scenes/
│   └── Main.tscn             # Escena principal con UI
├── Scripts/
│   ├── Main.cs               # Punto de entrada
│   ├── Core/
│   │   ├── BaseTool.cs       # Clase base para herramientas
│   │   ├── ToolManager.cs    # Gestor de herramientas
│   │   ├── HistoryManager.cs # Sistema undo/redo
│   │   ├── DrawingCanvas.cs  # Canvas de dibujo
│   │   ├── Layer.cs          # Clase de capa
│   │   └── LayerManager.cs   # Gestor de capas
│   ├── Tools/
│   │   ├── BrushTool.cs      # Herramienta Pincel
│   │   ├── EraserTool.cs     # Herramienta Borrador
│   │   ├── ColorPickerTool.cs# Herramienta Selector
│   │   ├── MoveTool.cs       # Herramienta Mover
│   │   └── SelectTool.cs     # Herramienta Selección
│   └── UI/
│       └── MainUI.cs         # Interfaz de usuario
└── Assets/
    └── icon.svg              # Recursos gráficos
```

## 🛠️ Crear Herramientas Personalizadas

La arquitectura está diseñada para ser extensible. Para crear una nueva herramienta:

```csharp
using Godot;
using PhotoGodot.Core;

namespace PhotoGodot.Tools;

public partial class MiHerramienta : BaseTool
{
    public override string Name => "MiHerramienta";
    public override string Description => "Descripción de mi herramienta";

    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Tu lógica de dibujo aquí
        if (WorkingLayer != null)
        {
            var pos = ScreenToLayer(to);
            WorkingLayer.DrawPixel((int)pos.X, (int)pos.Y, PrimaryColor, Opacity);
            WorkingLayer.UpdateTexture();
        }
    }
}
```

Luego registra la herramienta en `ToolManager.cs`:
```csharp
RegisterTool(new Tools.MiHerramienta());
```

## 📝 Notas Técnicas

- **Formato de imagen**: RGBA8 (32-bit con canal alpha)
- **Tamaño máximo de lienzo**: Limitado por memoria disponible
- **Rendimiento**: Optimizado para lienzos hasta 4096x4096
- **Thread safety**: Las operaciones de imagen son single-threaded

## 🐛 Solución de Problemas

### Error al cargar la escena
Asegúrate de usar Godot 4.6. Versiones anteriores pueden tener incompatibilidades.

### Bajo rendimiento
- Reduce el tamaño del canvas
- Disminuye el número de capas
- Desactiva el grid cuando no lo necesites

### Herramientas no responden
Verifica que haya al menos una capa visible y seleccionada.

## 📄 Licencia

Este proyecto es de código abierto. Úsalo libremente para tus proyectos.

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Siente libertad de mejorar las herramientas existentes o añadir nuevas características.

---

**Desarrollado con ❤️ usando Godot 4.6 y C#**

*PhotoGodot Pro v1.0 - ¡Tu estudio de diseño personal!*
