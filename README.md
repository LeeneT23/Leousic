# PhotoGodot Pro - Editor de Imágenes para Godot 4.6

![PhotoGodot Logo](icon.svg)

## Descripción

**PhotoGodot Pro** es una aplicación completa de edición de imágenes estilo Photoshop, desarrollada en C# para Godot 4.6. Ofrece herramientas profesionales de dibujo, gestión de capas, filtros y exportación de imágenes.

## ✨ Características Principales

### 🎨 Herramientas de Dibujo
- **Pincel**: Dibuja trazos libres con control de tamaño, opacidad y dureza
- **Borrador**: Elimina contenido con bordes suaves configurables
- **Selector de Color (Cuentagotas)**: Toma colores directamente del canvas
- **Mover**: Desplaza capas libremente
- **Selección**: Selecciona áreas rectangulares para operaciones específicas

### 📚 Sistema de Capas
- Múltiples capas ilimitadas (hasta 100 por defecto)
- Visibilidad individual por capa
- Modos de fusión: Normal, Multiplicar, Trama, Superponer, Oscurecer, Aclarar
- Opacidad ajustable por capa
- Bloqueo de capas
- Reordenamiento de capas
- Duplicado de capas
- Fusión de capas
- Aplanado de imagen

### 🔧 Filtros y Efectos
- Escala de grises
- Invertir colores
- Difuminar (Blur)
- Enfocar (Sharpen)
- Ajuste de brillo
- Ajuste de contraste

### ⌨️ Atajos de Teclado
| Acción | Atajo |
|--------|-------|
| Pincel | `B` |
| Borrador | `E` |
| Selector de color | `I` |
| Mover | `V` |
| Selección | `M` |
| Grid | `G` |
| Deshacer | `Ctrl + Z` |
| Rehacer | `Ctrl + Shift + Z` o `Ctrl + Y` |
| Nuevo documento | `Ctrl + N` |
| Guardar | `Ctrl + S` |
| Exportar | `Ctrl + E` |
| Nueva capa | `Ctrl + L` |
| Zoom in | `Ctrl +` |
| Zoom out | `Ctrl -` |
| Paneo | `Espacio + Arrastrar` o `Rueda central` |

### 💾 Formatos de Exportación
- PNG (con transparencia)
- JPG/JPEG
- WebP
- Proyecto nativo (.pgd)

## 🏗️ Arquitectura del Proyecto

```
PhotoGodot/
├── project.godot          # Configuración del proyecto
├── icon.svg               # Icono de la aplicación
├── README.md              # Este archivo
├── Scenes/
│   └── Main.tscn          # Escena principal
├── Scripts/
│   ├── Main.cs            # Punto de entrada principal
│   ├── Core/
│   │   ├── BaseTool.cs         # Clase base para herramientas
│   │   ├── ToolManager.cs      # Gestor de herramientas
│   │   ├── HistoryManager.cs   # Sistema undo/redo
│   │   ├── DrawingCanvas.cs    # Canvas de dibujo
│   │   ├── Layer.cs            # Clase de capa
│   │   └── LayerManager.cs     # Gestor de capas
│   ├── Tools/
│   │   ├── BrushTool.cs        # Herramienta pincel
│   │   ├── EraserTool.cs       # Herramienta borrador
│   │   ├── ColorPickerTool.cs  # Selector de color
│   │   ├── MoveTool.cs         # Herramienta mover
│   │   └── SelectTool.cs       # Herramienta selección
│   └── UI/
│       └── MainUI.cs           # Interfaz de usuario
├── Resources/             # Recursos adicionales
└── Shaders/               # Shaders personalizados
```

## 🚀 Cómo Usar

### Requisitos
- Godot 4.6 o superior
- .NET 6+ (para soporte de C#)

### Instalación
1. Clona o descarga este repositorio
2. Abre el proyecto en Godot 4.6
3. Presiona F5 para ejecutar

### Crear una Nueva Herramienta Personalizada

Para extender la aplicación con tus propias herramientas:

```csharp
using Godot;
using System.Collections.Generic;

public partial class MiHerramientaPersonalizada : BaseTool
{
    public MiHerramientaPersonalizada()
    {
        ToolName = "Mi Herramienta";
        ToolDescription = "Descripción de mi herramienta";
    }
    
    protected override void OnDrawStart(Vector2 position)
    {
        // Código al iniciar el trazo
    }
    
    protected override void OnDraw(Vector2 from, Vector2 to, Vector2 delta)
    {
        // Código durante el trazo
    }
    
    protected override void OnDrawEnd(Vector2 position)
    {
        // Código al finalizar el trazo
    }
}
```

Luego regístrala en el `ToolManager`:

```csharp
toolManager.RegisterTool(new MiHerramientaPersonalizada());
```

## 🎯 Funcionalidades Avanzadas

### Sistema de Historial
- Undo/Redo ilimitado (configurable, por defecto 100 acciones)
- Historial de todas las operaciones de dibujo
- Restauración completa del estado anterior

### Modos de Fusión
Implementación de algoritmos de mezcla de colores:
- **Normal**: Mezcla estándar con alpha
- **Multiply**: Oscurece multiplicando colores
- **Screen**: Aclara invirtiendo y multiplicando
- **Overlay**: Combina multiply y screen
- **Darken**: Mantiene el color más oscuro
- **Lighten**: Mantiene el color más claro

### Renderizado
- Composición en tiempo real de múltiples capas
- Soporte para zoom (10% - 1000%)
- Grid de referencia configurable
- Vista previa de alta calidad

## 📝 Notas de Desarrollo

### Optimizaciones Implementadas
- Lock/Unlock de imágenes para manipulación eficiente de píxeles
- Actualización diferida del canvas para mejor rendimiento
- Cache de texturas compuestas
- Dibujado incremental solo cuando es necesario

### Extensiones Futuras
- [ ] Herramienta de texto
- [ ] Formas geométricas predefinidas
- [ ] Degradados
- [ ] Patrones y texturas
- [ ] Transformaciones (rotar, escalar)
- [ ] Máscaras de capa
- [ ] Ajustes no destructivos
- [ ] Pinceles personalizados con texturas
- [ ] Soporte para tabletas gráficas con presión

## 📄 Licencia

Este proyecto es de código abierto. Siéntete libre de modificarlo y distribuirlo.

## 👨‍💻 Autor

Desarrollado como demostración de capacidades de desarrollo en Godot 4.6 con C#.

---

**¡Disfruta creando con PhotoGodot Pro! 🎨**
