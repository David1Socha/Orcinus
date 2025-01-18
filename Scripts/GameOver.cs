using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public class GameOver : Control
    {
        private PackedScene _homePackedScene;
        private Button _replayButton;

        [Signal]
        public delegate void SessionSaved();

        public override void _Ready()
        {
            Visible = false;

            _homePackedScene = GD.Load<PackedScene>("res://Scenes/Home.tscn");
            _replayButton = GetNode<Button>("Control/GameOverMenuBg/Grid/ReplayButton");

            GlobalSignalBus.RegisterEmitter(Signals.SessionSaved, this);
        }

        public async void OnGameOver()
        {
            GetTree().Paused = true;
            Visible = true;
            await TransitionHandler.FadeInFromCircle();

            ProgressTracker.SaveSessionProgress();
            EmitSignal(Signals.SessionSaved);

            _replayButton.GrabFocus();
        }

        public async void ReturnToHome()
        {
            await TransitionHandler.FadeToCircle();
            GetTree().Paused = false;
            GetTree().ChangeSceneTo(_homePackedScene);
        }
    }
}