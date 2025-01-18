using Godot;
using Orcinus.Scripts.CollectibleObjects;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Models.Missions;
using Orcinus.Scripts.Orcas;
using System;
using System.Collections.Generic;
using System.Linq;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public class Level : Node2D
    {
        [Signal]
        public delegate void DisplayedStatChanged();

        [Signal]
        public delegate void GameOver();

        [Signal]
        public delegate void ProgressLoaded();

        [Signal]
        public delegate void BiomeChanged(BiomeEnum biome);

        [Export]
        private readonly int PointsPerFish;

        [Export]
        private readonly int PointsPerNewBiome;

        [Export]
        private readonly int PointsPerPowerup;

        [Export]
        private readonly int PointsPerDistance;

        [Export]
        private readonly int PointsPerDestroyedObstacle;

        [Export]
        private readonly PackedScene[] TutorialSegments, TestLevelSegments, CoralLevelSegments, ArcticLevelSegments, CaveLevelSegments;

        [Export]
        private readonly PackedScene BiomeTransitionSegment;

        [Export]
        private readonly int[] BiomePaddings;

        [Export]
        private readonly int DistanceBetweenBiomes;
        [Export]
        private readonly float SpeedIncreasePerBiome;

        private Orca ActiveOrca => GetNode<Orca>("Orca");
        private RandomNumberGenerator _rng;
        private Node _collectiblesParent;
        private Control _pauseControl;
        private PowerupProgressBar _powerupBar;
        private AudioStreamPlayer _bgMusic;
        private Label _biomeLabel, _speedLabel;
        private BiomeEnum _biome;
        private BiomeEnum? _queuedBiome = null;
        private BiomeEnum? TerminalBiome;
        private MenuButton _soundButton, _musicButton;
        private MenuButton _vibrateButton;
        private float _timeElapsed = 0, _minHealthTimeElapsed = 0;
        private bool _isRestarting = false;
        private PowerupEffect _activePowerup = null;
        private int _orcaHealth = 3;
        private int ScoreMultiplier = 1;
        private bool _isQuipQueued = false;
        private int _tutorialStage = 0;
        private int? _missionsCompleteReminderDistanceThreshold = null;
        private bool _hasSentInitialMissionsReminder = false;

        private StatDictionary Stats => ProgressTracker.Progress.CurrentSessionStats;

        public override async void _Ready()
        {
            GetTree().Paused = true;
            CollectibleYachtPrize.YachtHasSpawned = false; // dumb lazy coding, but this is an easy way to get the functionality I want

            GlobalSignalBus.RegisterHandler(Signals.PrizeTriggered, this, nameof(OnPrizeTriggered));
            GlobalSignalBus.RegisterHandler(Signals.OrcaHealthChanged, this, nameof(OnOrcaHealthChanged));
            GlobalSignalBus.RegisterHandler(Signals.PowerupTriggered, this, nameof(OnPowerupTriggered));
            GlobalSignalBus.RegisterHandler(Signals.DisplayedStatChanged, this, nameof(OnDisplayedStatChanged));

            GlobalSignalBus.RegisterEmitter(Signals.DisplayedStatChanged, this);
            GlobalSignalBus.RegisterEmitter(Signals.GameOver, this);
            GlobalSignalBus.RegisterEmitter(Signals.ProgressLoaded, this);
            GlobalSignalBus.RegisterEmitter(Signals.BiomeChanged, this);

            _powerupBar = GetNode<PowerupProgressBar>("UILayer/InGameUI/ProgressBar");
            _powerupBar.Visible = false;
            _bgMusic = GetNode<AudioStreamPlayer>("BackgroundMusicPlayer");
            _biomeLabel = GetNode<Label>("UILayer/InGameUI/BiomeLabel");
            _biomeLabel.Visible = false;
            _speedLabel = GetNode<Label>("UILayer/InGameUI/SpeedLabel");
            _speedLabel.Visible = false;

            _soundButton = GetNode<MenuButton>("PauseLayer/Pause/Control/PauseMenuBg/VBoxContainer/HBoxContainer/SoundButton");
            _musicButton = GetNode<MenuButton>("PauseLayer/Pause/Control/PauseMenuBg/VBoxContainer/HBoxContainer/MusicButton");
            _vibrateButton = GetNode<MenuButton>("PauseLayer/Pause/Control/PauseMenuBg/VBoxContainer/HBoxContainer/VibrateButton");
            _musicButton.Pressed = Settings.MusicEnabled;
            _soundButton.Pressed = Settings.SoundEnabled;
            _vibrateButton.Pressed = Settings.VibrateEnabled;
            _vibrateButton.Visible = GDUtils.IsVibratingPlatform();
            _musicButton.Connect(BuiltInSignals.Toggled, Settings.Instance, nameof(Settings.SetMusicEnabled));
            _soundButton.Connect(BuiltInSignals.Toggled, Settings.Instance, nameof(Settings.SetSoundEnabled));
            _vibrateButton.Connect(BuiltInSignals.Toggled, Settings.Instance, nameof(Settings.SetVibrateEnabled));
            _pauseControl = GetNode<Control>("PauseLayer/Pause");

            ProgressTracker.LoadProgress();
            var discoverableOrca = ProgressTracker.Progress.UnlockableOrca;
            if (discoverableOrca != null)
            {
                GDUtils.FireAndForget(() => GDUtils.PrintTs("Orca " + discoverableOrca.GetDescription() + " can be discovered during this run"));
            }
            TerminalBiome = ProgressTracker.Progress.AllBiomesUnlocked ? (BiomeEnum?)null : ProgressTracker.Progress.MaxUnlockedBiome;
            ProgressTracker.Progress.CurrentSessionStats = new StatDictionary(autoPopulateAllKeys: true);
            EmitSignal(Signals.ProgressLoaded);
            GetTree().Paused = false; // we don't unpause until progress has been populated, to avoid race conditions in child update loops

            _collectiblesParent = GetNode<Node>("Collectibles");
            _rng = new RandomNumberGenerator();
            _rng.Randomize();

            _biome = Settings.StartingBiome;
            UpdateBiome(_biome, isLevelStart: true);
            EmitSignal(Signals.DisplayedStatChanged);

            await TransitionHandler.FadeInFromCircle();

            if (Settings.TutorialEnabled)
            {
                var segments = _collectiblesParent.GetChildren().Cast<Node>().ToList();
                foreach (var seg in segments)
                {
                    seg.QueueFree();
                }
            }
            else
            {
                if (Settings.ForceQuip || _rng.RandiRange(1, 3) == 1) // show quip with probability 1/3 (or always if ForceQuip is true)
                {
                    Settings.ForceQuip = false;
                    if (GetTree().Paused)
                    {
                        _isQuipQueued = true;
                    }
                    else
                    {
                        await DialogueHandler.PlayQuip(ActiveOrca);
                    }
                }
            }

            base._Ready();
        }

        public override void _ExitTree()
        {
            Engine.TimeScale = 1f;
            base._ExitTree();
        }

        public override async void _Process(float delta)
        {
            base._Process(delta);

            if (_isQuipQueued)
            {
                _isQuipQueued = false;
                await DialogueHandler.PlayQuip(ActiveOrca);
            }

            _timeElapsed += delta;
            Stats[StatEnum.Duration] = (int)_timeElapsed;
            if (_orcaHealth == 1)
            {
                _minHealthTimeElapsed += delta;
                Stats[StatEnum.MinHealthDuration] = (int)_minHealthTimeElapsed;
            }

            EmitSignal(Signals.DisplayedStatChanged);

            if (_activePowerup != null)
            {
                _powerupBar.Progress = 1 - (_activePowerup.DurationElapsed / _activePowerup.Duration);
                if (!_activePowerup.IsActive)
                {
                    OnPowerupDeactivated();
                    _powerupBar.Visible = false;
                    _activePowerup = null;
                }
            }

            if (Input.IsActionJustPressed("restart"))
            {
                RestartLevel();
            }

            if (Input.IsActionJustPressed("quit"))
            {
                this.Quit();
            }

            var inProgressMissions = ProgressTracker.Progress.Missions.Where(m => m.Status == MissionUnlockStatusEnum.IN_PROGRESS);
            bool currentMissionsDone = inProgressMissions.Any() && inProgressMissions.All(ipm => ipm.IsPendingUnlock(ProgressTracker.Progress));
            bool hasUnlockableMissions = ProgressTracker.Progress.Missions.Any(m => m.Status == MissionUnlockStatusEnum.LOCKED);
            if (currentMissionsDone && hasUnlockableMissions && !_hasSentInitialMissionsReminder)
            {
                _hasSentInitialMissionsReminder = true;
                await DialogueHandler.PlayDialogue(Dialogues.NewMissionsReady, ActiveOrca);
                _missionsCompleteReminderDistanceThreshold = Stats.GetValueOrDefault(StatEnum.Distance, 0) + 100;
            }

            if (_missionsCompleteReminderDistanceThreshold != null && Stats.GetValueOrDefault(StatEnum.Distance, 0) >= _missionsCompleteReminderDistanceThreshold)
            {
                await DialogueHandler.PlayDialogue(Dialogues.NewMissionsReadyReminder, ActiveOrca);
                _missionsCompleteReminderDistanceThreshold = Stats.GetValueOrDefault(StatEnum.Distance, 0) + 100;
            }
        }

        private void OnOrcaHealthChanged(int health)
        {
            _orcaHealth = health;
            if (health <= 0)
            {
                OnGameOver();
            }
        }

        public async void OnGameOver()
        {
            GetTree().Paused = true;
            // TODO play game over sound FX here !!

            await TransitionHandler.FadeToCircle(duration: .9f);
            _pauseControl.Visible = false;
            EmitSignal(Signals.GameOver);
        }

        private void OnPowerupDeactivated()
        {
            switch (_activePowerup.Type)
            {
                case PowerupEnum.SlowMotion:
                    Engine.TimeScale = 1f;
                    break;
                case PowerupEnum.DoubleFish:
                    ScoreMultiplier /= 2;
                    break;
                case PowerupEnum.TripleFish:
                    ScoreMultiplier /= 3;
                    break;
                default:
                    break;
            }
        }

        private void OnPowerupTriggered(PowerupEffect powerup)
        {
            IncrementDisplayedStat(StatEnum.Powerup);

            if (powerup.Duration != 0)
            {
                if (_activePowerup != null)
                {
                    OnPowerupDeactivated();
                }

                _powerupBar.ProgressColor = powerup.ModulateColor;
                _powerupBar.Progress = 1f;
                _powerupBar.PowerupIcon = powerup.PowerupIcon;
                _powerupBar.Visible = true;
                _activePowerup = powerup;
            }

            switch (powerup.Type)
            {
                case PowerupEnum.SlowMotion:
                    Engine.TimeScale = .7f;
                    break;
                case PowerupEnum.DoubleFish:
                    ScoreMultiplier *= 2;
                    break;
                case PowerupEnum.TripleFish:
                    ScoreMultiplier *= 3;
                    break;
                default:
                    break;
            }
        }

        public async void AdvanceTutorialIfEligible(float segmentXPos)
        {
            const int InitialBreakout = 0, SwimDown = 1, SwimUp = 2, DodgeObstacles = 3, Boost = 4, NetRescue = 5;
            GDUtils.PrintTs($"Checking if eligible to advance past stage {_tutorialStage}");
            bool succeeded;
            switch (_tutorialStage)
            {
                case InitialBreakout:
                    _tutorialStage++;
                    DialogueHandler.PlayDialogue(1, ActiveOrca);
                    AddLevelSegment(TutorialSegments[0], segmentXPos);
                    break;
                case SwimDown:
                    succeeded = Stats[StatEnum.Fish] == 1;
                    if (succeeded)
                    {
                        _tutorialStage++;
                        DialogueHandler.PlayDialogue(3, ActiveOrca);
                        AddLevelSegment(TutorialSegments[1], segmentXPos);
                    }
                    else
                    {
                        DialogueHandler.PlayDialogue(2, ActiveOrca);
                        AddLevelSegment(TutorialSegments[0], segmentXPos);
                    }
                    break;
                case SwimUp:
                    succeeded = Stats[StatEnum.Fish] == 2;
                    if (succeeded)
                    {
                        _tutorialStage++;
                        DialogueHandler.PlayDialogue(5, ActiveOrca);
                        AddLevelSegment(TutorialSegments[2], segmentXPos);
                    }
                    else
                    {
                        DialogueHandler.PlayDialogue(4, ActiveOrca);
                        AddLevelSegment(TutorialSegments[1], segmentXPos);
                    }
                    break;
                case DodgeObstacles:
                    succeeded = _orcaHealth == 3;
                    if (succeeded)
                    {
                        _tutorialStage++;
                        DialogueHandler.PlayDialogue(7, ActiveOrca);
                        AddLevelSegment(TutorialSegments[3], segmentXPos);
                    }
                    else
                    {
                        ActiveOrca.UpdateHealth(3);
                        DialogueHandler.PlayDialogue(6, ActiveOrca);
                        AddLevelSegment(TutorialSegments[2], segmentXPos);
                    }
                    break;
                case Boost:
                    succeeded = Stats[StatEnum.FishBoost] == 2;
                    if (succeeded)
                    {
                        _tutorialStage++;
                        DialogueHandler.PlayDialogue(9, ActiveOrca);
                        AddLevelSegment(TutorialSegments[4], segmentXPos, (segment) => segment.Connect(Signals.OrcaFreed, this, nameof(AdvanceTutorialIfEligible), new Godot.Collections.Array(new float[] { 0f })));
                    }
                    else
                    {
                        ActiveOrca.UpdateHealth(3);
                        DialogueHandler.PlayDialogue(8, ActiveOrca);
                        AddLevelSegment(TutorialSegments[3], segmentXPos);
                    }
                    break;
                case NetRescue:
                    var chatFish = this.GetNestedChildren().Where(node => node.IsInGroup(Groups.TutorialChattyFish)).FirstOrDefault() as CollectiblePrize;
                    if (chatFish == null)
                    {
                        GD.PrintErr($"Tutorial error: chat fish not found during final tutorial stage");
                        RestartLevel(leaveTutorial: true);
                    }
                    else
                    {
                        GD.Print("Tutorial successfully completed.");
                        chatFish.PitchScale = 1.5f;
                        var dialogueDict = new Dictionary<string, IDialogueSource>
                        {
                            ["Fish"] = chatFish,
                            ["Orca"] = ActiveOrca
                        };
                        await DialogueHandler.PlayDialogue(10, dialogueDict);
                        GetTree().Paused = true;
                        NotificationHandler.QueueNotification($"Tutorial Complete!", secondsBeforeDisplaying: 0f, secondsToDisplay: 1.5f);
                        await GDUtils.WaitAsync(this, 1.25f, PauseModeEnum.Process);
                        RestartLevel(leaveTutorial: true);
                    }
                    break;
                default:
                    GD.PrintErr($"Tutorial error: trying to transition from unexpected tutorial stage {_tutorialStage}");
                    RestartLevel(leaveTutorial: true);
                    break;
            }
        }

        public void RestartLevel()
        {
            RestartLevel(leaveTutorial: false);
        }

        public async void RestartLevel(bool leaveTutorial = false)
        {
            if (!_isRestarting)
            {
                _isRestarting = true;
                await TransitionHandler.FadeToCircle();
                if (leaveTutorial)
                {
                    Settings.Instance.SetTutorialEnabled(false);
                    var scene = GD.Load<PackedScene>("res://Scenes/Level.tscn");
                    GetTree().ChangeSceneTo(scene);
                }
                else
                {
                    GetTree().ReloadCurrentScene();
                }
            }
        }

        public void OnPrizeTriggered(CollectiblePrize prize)
        {
            int fishValue = prize.FishValue * ScoreMultiplier;

            if (prize.MagnetizingBody != null)
            {
                Stats.IncrementStat(StatEnum.MagnetizedFishCaught);
            }

            IncrementDisplayedStat(StatEnum.Fish, delta: fishValue);
        }

        private PackedScene[] GetSegmentPoolForBiome(BiomeEnum biome)
        {
            switch (biome)
            {
                case BiomeEnum.Test:
                    return TestLevelSegments;
                case BiomeEnum.Coral:
                    return CoralLevelSegments;
                case BiomeEnum.Arctic:
                    return ArcticLevelSegments;
                case BiomeEnum.Cave:
                    return CaveLevelSegments;
                default:
                    throw new Exception("Unexpected biome - no segments available");
            }
        }

        public void OnBiomeTransitionDetected()
        {
            UpdateBiome(_queuedBiome.Value);
            IncrementDisplayedStat(StatEnum.Biome);
            _queuedBiome = null;
        }

        public void OnLevelSegmentEndDetected(float segmentEndXPosition)
        {
            if (Settings.TutorialEnabled)
            {
                AdvanceTutorialIfEligible(segmentEndXPosition);
            }
            else
            {
                var hasAdditionalReachableBiomes = _biome != TerminalBiome;
                int biomeOffset = 2 + Stats[StatEnum.Biome] - (int)Settings.StartingBiome;
                var hasFullyTraveledCurrentBiome = DistanceBetweenBiomes * biomeOffset < Stats.GetValueOrDefault(StatEnum.Distance, 0);
                if (hasAdditionalReachableBiomes && _queuedBiome == null && hasFullyTraveledCurrentBiome)
                {
                    bool isInfiniteMode = TerminalBiome == null;
                    if (isInfiniteMode)
                    {
                        var possibleBiomes = EnumExtensions.GetEnumValues<BiomeEnum>();
                        var newBiomes = possibleBiomes.Where(b => b != _biome && b != BiomeEnum.Test).ToArray();
                        _queuedBiome = newBiomes.PickRandomElement(_rng);
                    }
                    else
                    {
                        _queuedBiome = _biome + 1;
                    }

                    AddLevelSegment(BiomeTransitionSegment, segmentEndXPosition, (node) => (node as TransitionSegment).AssociatedBiome = _queuedBiome.Value);
                }
                else
                {
                    var segmentPool = GetSegmentPoolForBiome(_biome);
                    var nextSegmentScene = segmentPool.PickRandomElement(_rng);
                    AddLevelSegment(nextSegmentScene, segmentEndXPosition);
                }
            }
        }

        private void AddLevelSegment(PackedScene segmentScene, float currentSegmentEndXPosition, Action<Node2D> postInstanceAction = null)
        {
            var nextSegment = segmentScene.Instance<Node2D>();
            if (postInstanceAction != null)
            {
                postInstanceAction(nextSegment);
            }

            var abstractPowerups = nextSegment.GetChildren().Cast<Node>().Where(n => n as AbstractCollectiblePowerup != null);
            foreach (var powerup in abstractPowerups)
            {
                (powerup as AbstractCollectiblePowerup).OrcaHealth = _orcaHealth;
            }

            var biomePadding = BiomePaddings[(int)_biome];
            nextSegment.Position = new Vector2(currentSegmentEndXPosition + biomePadding, 0);
            GDUtils.PrintTs($"Adding level segment: {nextSegment.Name} at X position: {currentSegmentEndXPosition}");
            _collectiblesParent.AddChildDeferred(nextSegment);
        }

        private void UpdateBiome(BiomeEnum biome, bool isLevelStart = false)
        {
            GDUtils.PrintTs($"Now entering {biome.GetDescription()} biome");
            _biomeLabel.Text = biome.GetDescription();
            _biomeLabel.Visible = !Settings.TutorialEnabled;
            _biome = biome;
            EmitSignal(Signals.BiomeChanged, _biome);

            var tween = CreateTween();
            tween.TweenInterval(1f);
            tween.TweenProperty(_biomeLabel, "visible", false, 0f);
            if (!isLevelStart)
            {
                tween.TweenProperty(_speedLabel, "visible", true, 0f);
                tween.TweenCallback(ActiveOrca, "IncreaseHorizontalSpeed", new Godot.Collections.Array(new float[] { SpeedIncreasePerBiome }));
                tween.TweenInterval(1f);
                tween.TweenProperty(_speedLabel, "visible", false, 0f);
            }
        }

        public void IncrementDisplayedStat(StatEnum statType, int delta = 1)
        {
            Stats.IncrementStat(statType, delta);

            EmitSignal(Signals.DisplayedStatChanged);
        }

        private void OnDisplayedStatChanged()
        {
            Stats[StatEnum.Points] = RecalculatePoints();
        }

        private int RecalculatePoints()
        {
            var fishCount = Stats.GetValueOrDefault(StatEnum.Fish, 0);
            var biomeCount = Stats.GetValueOrDefault(StatEnum.Biome, 0);
            var powerupCount = Stats.GetValueOrDefault(StatEnum.Powerup, 0);
            var distanceCount = Stats.GetValueOrDefault(StatEnum.Distance, 0);
            var destroyedObstacleCount = Stats.GetValueOrDefault(StatEnum.DestroyedObstacle, 0);

            var fishPoints = fishCount * PointsPerFish;
            var biomePoints = biomeCount * PointsPerNewBiome;
            var powerupPoints = powerupCount * PointsPerPowerup;
            var distancePoints = distanceCount * PointsPerDistance;
            var destructionPoints = destroyedObstacleCount * PointsPerDestroyedObstacle;

            var score = fishPoints + biomePoints + powerupPoints + distancePoints + destructionPoints;
            return score;
        }
    }
}
