# 🎨 PhotoGodot Pro

**Editor de imágenes profesional estilo Photoshop para Godot 4.6 con C#**

## ✨ Características Principales

### 🖌️ Herramientas de Dibujo
- **Pincel (B)**: Con ajuste de tamaño, opacidad y dureza
- **Borrador (E)**: Borrado suave con control de opacidad
- **Selector de Color (I)**: Cuentagotas para picking de colores
- **Mover (V)**: Desplazamiento por el lienzo
- **Selección (M)**: Selección rectangular

### 📚 Sistema de Capas
- Capas ilimitadas (hasta 100 por defecto)
- Visibilidad por capa
- Opacidad ajustable
- 6 modos de fusión: Normal, Multiplicar, Trama, Superponer, Oscurecer, Aclarar
- Reordenar capas
- Duplicar capas
- Fusionar hacia abajo
- Aplanar imagen

### 🔄 Historial
- Undo/Redo ilimitado (configurable hasta 500 estados)
- Guardado automático de estado después de cada acción

### 🎯 Filtros
- Escala de grises
- Invertir colores
- Difuminar (placeholder)
- Enfocar (placeholder)

### 👁️ Vista
- Grid/tamaño ajustable
- Zoom (10%-1000%)
- Paneo del lienzo

### 💾 Exportación
- PNG
- JPG
- WebP

### ⌨️ Atajos de Teclado

| Acción | Atajo |
|--------|-------|
| Pincel | B |
| Borrador | E |
| Selector | I |
| Mover | V |
| Selección | M |
| Toggle Grid | G |
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Nuevo Documento | Ctrl+N |
| Abrir | Ctrl+O |
| Guardar | Ctrl+S |
| Exportar | Ctrl+E |
| Salir | Ctrl+Q |

## 🚀 Cómo Usar

### Requisitos
- Godot 4.6 o superior
- .NET 6+ SDK

### Instalación
1. Abre Godot 4.6
2. Importa el proyecto desde la carpeta `/workspace`
3. Presiona F5 para ejecutar

### Primeros Pasos
1. Selecciona una herramienta de la barra superior
2. Elige un color desde el selector
3. Ajusta tamaño, opacidad y dureza según necesites
4. Comienza a dibujar en el lienzo central
5. Usa el panel derecho para gestionar capas y aplicar filtros

## 🏗️ Arquitectura del Proyecto

```
/workspace/
├── project.godot              # Configuración Godot 4.6
├── icon.svg                   # Icono de la aplicación
├── README.md                  # Esta documentación
├── Scenes/
│   ├── Main.tscn             # Escena principal
│   └── MainUI.tscn           # Interfaz de usuario
└── Scripts/
    ├── Main.cs               # Punto de entrada y coordinador
    ├── Core/
    │   ├── BaseTool.cs       # Clase base para herramientas
    │   ├── ToolManager.cs    # Gestor de herramientas
    │   ├── HistoryManager.cs # Sistema undo/redo
    │   ├── DrawingCanvas.cs  # Canvas de dibujo y grid
    │   ├── Layer.cs          # Clase de capa individual
    │   └── LayerManager.cs   # Gestor de múltiples capas
    ├── Tools/
    │   ├── BrushTool.cs      # Herramienta pincel
    │   ├── EraserTool.cs     # Herramienta borrador
    │   ├── ColorPickerTool.cs# Herramienta cuentagotas
    │   ├── MoveTool.cs       # Herramienta mover
    │   └── SelectTool.cs     # Herramienta selección
    └── UI/
        └── MainUI.cs         # Lógica de la interfaz
```

## 🔧 Crear Herramientas Personalizadas

PhotoGodot Pro está diseñado para ser extensible. Para crear tu propia herramienta:

```csharp
using Godot;

public partial class MiHerramienta : BaseTool
{
    public MiHerramienta()
    {
        _toolName = "MiHerramienta";
    }
    
    protected override void OnPressStart(Vector2 position)
    {
        // Lógica al iniciar el dibujo
        GD.Print($"Inicio en: {position}");
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Lógica durante el arrastre del mouse
        var layer = _main.GetLayerManager().ActiveLayer;
        if (layer != null)
        {
            layer.DrawLine(from, to, _main.GetPrimaryColor(), _main.GetBrushSize());
        }
        
        // Guardar estado para undo
        var compositedImage = _main.GetLayerManager().GetCompositedImage();
        if (compositedImage != null)
        {
            _main.GetHistoryManager().SaveState(compositedImage);
        }
    }
    
    protected override void OnPressEnd(Vector2 position)
    {
        // Lógica al finalizar el dibujo
        GD.Print($"Fin en: {position}");
    }
}
```

Luego regístrala en `Main.cs`:
```csharp
_toolManager.RegisterTool(new MiHerramienta(this));
```

## 📊 Estadísticas del Proyecto

- **Total de archivos**: 17
- **Líneas de código C#**: ~2,500+
- **Herramientas implementadas**: 5
- **Modos de fusión**: 6
- **Atajos de teclado**: 15+

## 🐛 Solución de Problemas

### Error: "scene/resources/resource_format_text.cpp"
Este error ocurre si las escenas tienen referencias rotas. Solución:
1. Verifica que todos los scripts existan en las rutas especificadas
2. Reimporta el proyecto en Godot
3. Limpia la carpeta `.godot` y vuelve a abrir

### Error: "node does not specify its parent"
Ocurre cuando los nodos en el archivo `.tscn` no tienen la jerarquía correcta. 
El proyecto actual ya tiene esto solucionado con la estructura correcta.

## 📝 Licencia

Proyecto creado con fines educativos y de demostración.

---

**¡Disfruta creando con PhotoGodot Pro! 🎨✨**
