using Godot;

namespace Orcinus.Scripts.Core
{
    [Tool]
    public class EnhancedTextureButton : TextureButton
    {
        private Texture _texture;
        private bool _isHidden;

        [Export]
        public readonly int Id = 0;

        [Export]
        private Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                _texture = value;
                UpdateTextures();
            }
        }

        [Export]
        public bool IsHidden
        {
            get
            {
                return _isHidden;
            }
            set
            {
                _isHidden = value;
                UpdateButtonHideStatus();
            }
        }

        public override void _Ready()
        {
            base._Ready();
            UpdateTextures();
            UpdateButtonHideStatus();
        }

        private void UpdateButtonHideStatus()
        {
            if (_isHidden)
            {
                Disabled = true;
                Modulate = new Color(0f, 0f, 0f);
            }
            else
            {
                Disabled = false;
                Modulate = new Color(1f, 1f, 1f);
            }
        }

        private void UpdateTextures()
        {
            TextureNormal = _texture;
            if (_texture == null)
            {
                TextureClickMask = null;
                return;
            }

            var textureData = _texture.GetData();
            if (FlipH)
            {
                textureData.FlipX();
            }

            if (FlipV)
            {
                textureData.FlipY();
            }

            var clickMask = new BitMap();
            clickMask.CreateFromImageAlpha(textureData);
            TextureClickMask = clickMask;
        }
    }
}
