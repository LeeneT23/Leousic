using Godot;
using System;

namespace PhotoGodot.Core
{
    /// <summary>
    /// Represents a single layer in the layer stack
    /// </summary>
    public class Layer : Node
    {
        [Signal] public delegate void LayerChangedEventHandler();
        [Signal] public delegate void LayerPropertyChangedEventHandler(string propertyName);
        
        private Image _image;
        private ImageTexture _texture;
        private string _layerName;
        private bool _isVisible = true;
        private float _opacity = 1.0f;
        private BlendMode _blendMode = BlendMode.Normal;
        
        public enum BlendMode
        {
            Normal,
            Multiply,
            Screen,
            Overlay,
            Darken,
            Lighten,
            ColorDodge,
            ColorBurn,
            HardLight,
            SoftLight,
            Difference,
            Exclusion,
            Hue,
            Saturation,
            Color,
            Luminosity
        }
        
        public string LayerName 
        { 
            get => _layerName; 
            set 
            { 
                _layerName = value; 
                EmitSignal(SignalName.LayerPropertyChanged, "name");
            } 
        }
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set 
            { 
                _isVisible = value; 
                EmitSignal(SignalName.LayerPropertyChanged, "visibility");
                EmitSignal(SignalName.LayerChanged);
            } 
        }
        
        public float Opacity 
        { 
            get => _opacity; 
            set 
            { 
                _opacity = Mathf.Clamp(value, 0f, 1f); 
                EmitSignal(SignalName.LayerPropertyChanged, "opacity");
                EmitSignal(SignalName.LayerChanged);
            } 
        }
        
        public BlendMode CurrentBlendMode 
        { 
            get => _blendMode; 
            set 
            { 
                _blendMode = value; 
                EmitSignal(SignalName.LayerPropertyChanged, "blendMode");
                EmitSignal(SignalName.LayerChanged);
            } 
        }
        
        public Image Image => _image;
        public ImageTexture Texture => _texture;
        public int Width => _image?.GetWidth() ?? 0;
        public int Height => _image?.GetHeight() ?? 0;
        
        /// <summary>
        /// Create a new layer with specified dimensions
        /// </summary>
        public void Create(int width, int height, bool transparent = true)
        {
            _image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            
            if (!transparent)
            {
                _image.Fill(Colors.White);
            }
            else
            {
                _image.Fill(new Color(0, 0, 0, 0));
            }
            
            _texture = ImageTexture.CreateFromImage(_image);
            
            GD.Print($"[Layer] Created layer '{_layerName}' ({width}x{height})");
        }
        
        /// <summary>
        /// Create layer from existing image
        /// </summary>
        public void FromImage(Image sourceImage, string name = "Layer")
        {
            _layerName = name;
            _image = sourceImage.Duplicate();
            _texture = ImageTexture.CreateFromImage(_image);
            
            GD.Print($"[Layer] Created from image: '{_layerName}'");
        }
        
        /// <summary>
        /// Clear the layer content
        /// </summary>
        public void Clear(Color fillColor)
        {
            if (_image == null) return;
            
            _image.Lock();
            _image.Fill(fillColor);
            _image.Unlock();
            
            UpdateTexture();
            EmitSignal(SignalName.LayerChanged);
        }
        
        /// <summary>
        /// Update texture after image modifications
        /// </summary>
        public void UpdateTexture()
        {
            if (_texture != null && _image != null)
            {
                _texture.Update(_image);
            }
        }
        
        /// <summary>
        /// Apply blend mode to composite this layer onto another
        /// </summary>
        public void CompositeOnto(Image target, int offsetX = 0, int offsetY = 0)
        {
            if (!_isVisible || _image == null || target == null) return;
            
            _image.Lock();
            target.Lock();
            
            int startX = Math.Max(0, offsetX);
            int startY = Math.Max(0, offsetY);
            int endX = Math.Min(Width, target.GetWidth() - offsetX);
            int endY = Math.Min(Height, target.GetHeight() - offsetY);
            
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Color srcColor = _image.GetPixel(x, y);
                    
                    if (srcColor.A == 0) continue;
                    
                    Color dstColor = target.GetPixel(x + offsetX, y + offsetY);
                    Color blended = ApplyBlendMode(dstColor, srcColor, _blendMode);
                    blended.A = Mathf.Min(dstColor.A + srcColor.A * _opacity, 1f);
                    
                    target.SetPixel(x + offsetX, y + offsetY, blended);
                }
            }
            
            _image.Unlock();
            target.Unlock();
        }
        
        /// <summary>
        /// Apply blend mode formula
        /// </summary>
        private Color ApplyBlendMode(Color baseColor, Color blendColor, BlendMode mode)
        {
            float alpha = blendColor.A * _opacity;
            
            switch (mode)
            {
                case BlendMode.Normal:
                    return BlendNormal(baseColor, blendColor, alpha);
                    
                case BlendMode.Multiply:
                    return BlendMultiply(baseColor, blendColor, alpha);
                    
                case BlendMode.Screen:
                    return BlendScreen(baseColor, blendColor, alpha);
                    
                case BlendMode.Darken:
                    return BlendDarken(baseColor, blendColor, alpha);
                    
                case BlendMode.Lighten:
                    return BlendLighten(baseColor, blendColor, alpha);
                    
                default:
                    return BlendNormal(baseColor, blendColor, alpha);
            }
        }
        
        private Color BlendNormal(Color baseC, Color blendC, float alpha)
        {
            return new Color(
                baseC.R + (blendC.R - baseC.R) * alpha,
                baseC.G + (blendC.G - baseC.G) * alpha,
                baseC.B + (blendC.B - baseC.B) * alpha,
                baseC.A + blendC.A * alpha
            );
        }
        
        private Color BlendMultiply(Color baseC, Color blendC, float alpha)
        {
            return new Color(
                baseC.R + (baseC.R * blendC.R - baseC.R) * alpha,
                baseC.G + (baseC.G * blendC.G - baseC.G) * alpha,
                baseC.B + (baseC.B * blendC.B - baseC.B) * alpha,
                baseC.A + blendC.A * alpha
            );
        }
        
        private Color BlendScreen(Color baseC, Color blendC, float alpha)
        {
            return new Color(
                baseC.R + (1 - (1 - baseC.R) * (1 - blendC.R) - baseC.R) * alpha,
                baseC.G + (1 - (1 - baseC.G) * (1 - blendC.G) - baseC.G) * alpha,
                baseC.B + (1 - (1 - baseC.B) * (1 - blendC.B) - baseC.B) * alpha,
                baseC.A + blendC.A * alpha
            );
        }
        
        private Color BlendDarken(Color baseC, Color blendC, float alpha)
        {
            return new Color(
                baseC.R + (Math.Min(baseC.R, blendC.R) - baseC.R) * alpha,
                baseC.G + (Math.Min(baseC.G, blendC.G) - baseC.G) * alpha,
                baseC.B + (Math.Min(baseC.B, blendC.B) - baseC.B) * alpha,
                baseC.A + blendC.A * alpha
            );
        }
        
        private Color BlendLighten(Color baseC, Color blendC, float alpha)
        {
            return new Color(
                baseC.R + (Math.Max(baseC.R, blendC.R) - baseC.R) * alpha,
                baseC.G + (Math.Max(baseC.G, blendC.G) - baseC.G) * alpha,
                baseC.B + (Math.Max(baseC.B, blendC.B) - baseC.B) * alpha,
                baseC.A + blendC.A * alpha
            );
        }
        
        /// <summary>
        /// Export layer as image
        /// </summary>
        public Image Export()
        {
            return _image?.Duplicate();
        }
        
        /// <summary>
        /// Save layer to file
        /// </summary>
        public Error SaveToFile(string path)
        {
            if (_image == null) return Error.InvalidData;
            return _image.SavePng(path);
        }
    }
}
