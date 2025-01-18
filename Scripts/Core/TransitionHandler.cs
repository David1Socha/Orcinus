using Godot;
using System.Threading.Tasks;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.Core
{
    public class TransitionHandler : CanvasLayer
    {
        private ColorRect _fadeToBlack, _circleRect;
        private static TransitionHandler Instance;
        private SceneTreeTween _activeTween = null;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;

            _fadeToBlack = GetNode<ColorRect>("FadeToBlackRectangle");
            _circleRect = GetNode<ColorRect>("CircleRect");
        }

        public static async Task FadeToCircle(float duration = .6f)
        {
            await Instance.FadeToCircleInternal(duration);
        }

        private void ClearPreviousTween()
        {
            if (_activeTween != null && _activeTween.IsRunning())
            {
                _activeTween.CustomStep(100000);
                _activeTween = null;
            }
        }

        public async Task FadeToCircleInternal(float duration)
        {
            ClearPreviousTween();
            var tween = CreateTween();
            _activeTween = tween;

            _circleRect.Visible = true;
            var material = _circleRect.Material.AsShaderMaterial();
            material.SetShaderParam(ShaderParams.CircleSize, .99f);
            tween.TweenProperty(material, ShaderParams.GetParamNodePath(ShaderParams.CircleSize), .25f, duration);
            tween.TweenProperty(_circleRect, "visible", false, 0f);
            await this.WaitForTweenCompletion(tween);
        }

        public static async Task FadeInFromCircle(float duration = .6f)
        {
            await Instance.FadeInFromCircleInternal(duration);
        }

        public async Task FadeInFromCircleInternal(float duration)
        {
            ClearPreviousTween();
            var tween = CreateTween();
            _activeTween = tween;

            _circleRect.Visible = true;
            var material = _circleRect.Material.AsShaderMaterial();
            material.SetShaderParam(ShaderParams.CircleSize, .25f);
            tween.TweenProperty(material, ShaderParams.GetParamNodePath(ShaderParams.CircleSize), .99f, duration);
            tween.TweenProperty(_circleRect, "visible", false, 0f);
            await this.WaitForTweenCompletion(tween);
        }
    }
}
