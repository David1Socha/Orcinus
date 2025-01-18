using Godot;
using Orcinus.Scripts.Core;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{

    public class Pause : Control
    {
        private Button _resumeButton;
        private PackedScene _homePackedScene;
        private bool _canReceivePauses;

        [Signal]
        public delegate void PauseTriggered();

        [Signal]
        public delegate void UnpauseTriggered();

        public override void _Ready()
        {
            Visible = false;
            _canReceivePauses = true;

            _resumeButton = GetNode<Button>("Control/PauseMenuBg/VBoxContainer/ResumeButton");
            _homePackedScene = GD.Load<PackedScene>("res://Scenes/Home.tscn");

            GlobalSignalBus.RegisterEmitter(Signals.PauseTriggered, this);
            GlobalSignalBus.RegisterEmitter(Signals.UnpauseTriggered, this);

            SteamWrapper.AddOverlayHandler(OnGameOverlayActivated);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            SteamWrapper.RemoveOverlayHandler(OnGameOverlayActivated);
        }

        public void OnGameOverlayActivated(bool isOverlayOpen)
        {
            if (isOverlayOpen)
            {
                OnPauseTriggered();
            }
        }

        public void OnPauseTriggered()
        {
            GetTree().Paused = true;
            Visible = true;
            EmitSignal(Signals.PauseTriggered);

            _resumeButton.GrabFocus();
        }

        public void OnUnpauseTriggered()
        {
            EmitSignal(Signals.UnpauseTriggered);
            GetTree().Paused = false;
            Visible = false;
        }

        public async void ReturnToHome()
        {
            Engine.TimeScale = 1f;
            await TransitionHandler.FadeToCircle();
            GetTree().Paused = false;
            GetTree().ChangeSceneTo(_homePackedScene);
        }

        public void OnGameOver()
        {
            // disable pause button and pause menu once the user has reached game over
            _canReceivePauses = false;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);

            if (@event.IsActionPressed("pause") && _canReceivePauses)
            {
                if (GetTree().Paused && Visible)
                {
                    OnUnpauseTriggered();
                }
                else if (!GetTree().Paused)
                {
                    OnPauseTriggered();
                }
            }

            @event.Dispose();
        }

        public override void _Notification(int what)
        {
            // This fn pauses the game on unfocus, which is what we want for the actual game. However, when debugging, it's nice to have this disabled

            bool shouldNotificationPause = what == MainLoop.NotificationWmFocusOut || what == MainLoop.NotificationAppPaused || what == MainLoop.NotificationWmGoBackRequest;
            if (shouldNotificationPause && _canReceivePauses && !GetTree().Paused)
            {
                OnPauseTriggered();
            }
        }
    }

}