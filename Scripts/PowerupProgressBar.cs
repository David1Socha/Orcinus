using Godot;

namespace Orcinus.Scripts
{
    [Tool]
    public class PowerupProgressBar : Core.ProgressBar
    {
        private TextureRect _powerupIcon;
        private Texture _powerupTexture;

        [Export]
        public Texture PowerupIcon
        {
            get
            {
                return _powerupTexture;
            }
            set
            {
                _powerupTexture = value;
                if (_powerupIcon != null)
                {
                    _powerupIcon.Texture = value;
                }
            }
        }

        public override void _Ready()
        {
            base._Ready();

            _powerupIcon = GetNode<TextureRect>("PowerupIcon");
            _powerupIcon.Texture = _powerupTexture;
            _powerupIcon.RectPosition = new Vector2(-20f, -10f);
            _powerupIcon.RectSize = new Vector2(RectSize.y + 20f, RectSize.y + 20f);
        }
    }
}
