using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public class Home : Node2D
    {
        private VBoxContainer _buttonContainer;
        private MenuButton _playButton, _creditsButton, _quitButton, _progressButton;
        private PopupPanel _creditsPopup;
        private ProgressDisplay _progressDisplay;
        private Camera2D _camera;
        private PackedScene _gameScene;
        private bool _playLevelOnceLoaded;
        private bool _platformDisallowsQuit;

        [Signal]
        public delegate void BiomeChanged(BiomeEnum biome);

        [Signal]
        public delegate void ProgressLoaded();

        public async override void _Ready()
        {
            base._Ready();

            ProgressTracker.LoadProgress();

            var platformName = OS.GetName();
            _platformDisallowsQuit = platformName == "HTML5" || platformName == "iOS" || platformName == "Android";
            _quitButton = GetNode<MenuButton>("MainMenuLayer/MainMenu/ButtonsBackground/VBoxContainer/QuitButton");
            if (_platformDisallowsQuit)
            {
                _quitButton.Visible = false;
            }

            GlobalSignalBus.RegisterEmitter(Signals.BiomeChanged, this);
            GlobalSignalBus.RegisterEmitter(Signals.ProgressLoaded, this);

            _creditsButton = GetNode<MenuButton>("MainMenuLayer/MainMenu/ButtonsBackground/VBoxContainer/CreditsButton");
            _playButton = GetNode<MenuButton>("MainMenuLayer/MainMenu/ButtonsBackground/VBoxContainer/PlayButton");
            _playButton.GrabFocus();
            _progressButton = GetNode<MenuButton>("MainMenuLayer/MainMenu/ButtonsBackground/VBoxContainer/ProgressButton");

            _buttonContainer = GetNode<VBoxContainer>("MainMenuLayer/MainMenu/ButtonsBackground/VBoxContainer");
            _creditsPopup = GetNode<PopupPanel>("MainMenuLayer/MainMenu/CreditsPopup");
            _progressDisplay = GetNode<ProgressDisplay>("MainMenuLayer/MainMenu/ProgressDisplay");
            _camera = GetNode<Camera2D>("Camera2D");

            var rng = new RandomNumberGenerator();
            rng.Randomize();
            var mainMenuBiome = EnumExtensions.GetRandomValue<BiomeEnum>(rng);
            EmitSignal(Signals.BiomeChanged, mainMenuBiome);
            EmitSignal(Signals.ProgressLoaded);

            GDUtils.FireAndForget(PreLoadLevelScene);
            await TransitionHandler.FadeInFromCircle();
            await GDUtils.Wait(this, .75f);
            _camera.Position = new Vector2(1000000f, 0f);
        }

        private void PreLoadLevelScene()
        {
            var scenePath = Settings.TutorialEnabled ? "res://Scenes/TutorialLevel.tscn" : "res://Scenes/Level.tscn";
            _gameScene = GD.Load<PackedScene>(scenePath);
            if (_playLevelOnceLoaded)
            {
                CallDeferred(nameof(PlayLevel));
            }
        }

        public void QuitGame()
        {
            if (!_platformDisallowsQuit)
            {
                SteamWrapper.ShutdownIfActive();
                this.Quit();
            }

        }
        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmGoBackRequest)
                QuitGame();
        }

        public async void PlayLevel()
        {
            if (_gameScene != null)
            {
                await TransitionHandler.FadeToCircle();
                GetTree().ChangeSceneTo(_gameScene);
            }
            else
            {
                _playLevelOnceLoaded = true;
                GetTree().Paused = true;
            }
        }

        public void ShowCredits()
        {
            _creditsPopup.PopupCenteredRatio(.5f);
            _buttonContainer.Visible = false;
        }

        public void ShowProgress()
        {
            _progressDisplay.Visible = true;
            _buttonContainer.Visible = false;
        }

        public void CloseCredits()
        {
            _creditsPopup.Hide();
        }

        public void OnCreditsClosed()
        {
            _buttonContainer.Visible = true;
            _creditsButton.GrabFocus();
        }

        public void OnProgressDisplayClosed()
        {
            _buttonContainer.Visible = true;
            _progressButton.GrabFocus();
        }

        public void OnSeaPandaGuiInput(InputEvent input)
        {
            var sp = GetNode<Control>("MainMenuLayer/MainMenu/SeaPandaPanel");
            if (input.IsReleasedWithinControl(sp))
            {
                OS.ShellOpen("https://seapanda.xyz/");
            }
        }

        public void OnGodotGuiInput(InputEvent input)
        {
            var gd = GetNode<Control>("MainMenuLayer/MainMenu/GodotPanel");
            if (input.IsReleasedWithinControl(gd))
            {
                OS.ShellOpen("https://godotengine.org/");
            }
        }
    }
}
