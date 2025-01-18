using Godot;

namespace Orcinus.Scripts.Core
{
    [Tool]
    public class ProgressBar : Control
    {
        protected ColorRect _progressRect;
        protected ColorRect _backgroundRect;

        public override void _Ready()
        {
            base._Ready();

            _progressRect = GetNode<ColorRect>("ProgressRect");
            _backgroundRect = GetNode<ColorRect>("BackgroundRect");

            _backgroundRect.Color = _backgroundColor;
            _progressRect.Color = _progressColor;
            _progressRect.AnchorRight = _progress;
        }

        private Color _backgroundColor, _progressColor;
        private float _progress;

        [Export]
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; if (_backgroundRect != null) _backgroundRect.Color = value; } }

        [Export]
        public Color ProgressColor { get { return _progressColor; } set { _progressColor = value; if (_progressRect != null) _progressRect.Color = value; } }

        [Export(PropertyHint.Range, "0,1")]
        public float Progress { get { return _progress; } set { _progress = value; if (_progressRect != null) _progressRect.AnchorRight = value; } }
    }
}
