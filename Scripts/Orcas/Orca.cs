using Godot;
using Orcinus.Scripts.CollectibleObjects;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using System;
using System.Linq;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.Orcas
{
    public class Orca : KinematicBody2D, IDialogueSource
    {

        [Export]
        private readonly float HorizontalSpeed;

        [Export]
        private readonly float VerticalAccelerationOnPress;

        [Export]
        private readonly float MaxVerticalSpeed;

        [Export]
        private readonly float MaxRotationRadians;

        [Export]
        private readonly float RotationSpeedRadians;

        [Export]
        private readonly int UnitsPerMeter;

        [Signal]
        public delegate void OrcaHealthChanged(int health);

        [Signal]
        public delegate void DisplayedStatChanged();

        [Export]
        private readonly int StartingHealth;

        [Export]
        private readonly float InvulnerabilityTimeAfterDamaged;

        [Export]
        private readonly float FishStreakMaxTimeDelay;

        [Export]
        public NodePath PositionFollower;

        [Export]
        public int FishBoostThreshold;

        [Export]
        public float FishBoostDuration;

        [Export]
        public float FishBoostXSpeedIncrease;

        [Export]
        public float FishBoostGracePeriod;

        [Export]
        private readonly float BasePrizePitch;

        [Export]
        private readonly int BoostTrailMinSegments;

        [Export]
        private readonly int BoostTrailMaxSegments;

        [Export]
        private readonly OrcaEnum OrcaEnum;

        [Export]
        private readonly Color DialogueColor;

        [Export]
        private readonly bool IsPlaceholder = false;

        [Export]
        public Vector2 Velocity;

        private bool _hasTakenDamageInCurrentBiome = false;

        public HatEnum ActiveHat { get; set; } = HatEnum.None;

        public Vector2 DialogueScreenPosition => _dialogueSource.GetGlobalTransformWithCanvas().origin;
        public float PitchScale => BasePrizePitch;

        public string DialogueName => OrcaEnum.GetDescription();
        Color? IDialogueSource.DialogueColor => DialogueColor;

        private int BoostTrailSegmentRange { get { return BoostTrailMaxSegments - BoostTrailMinSegments; } }

        private Node2D _dialogueSource;
        private int _health;

        private int _nearbyConsumablesCount;
        private CollisionState _collisionState;
        private AnimatedSprite _orcaMouth;
        protected AnimatedSprite _orcaBody;
        private Node2D _finlet1, _finlet2;
        private AnimationPlayer _effectsPlayer;
        private RemoteTransform2D _remoteTransform;
        private float _remainingInvulnerabilityTime;
        private OrcaBubbleEmitter _bubbleEmitter;
        private Node2D _speedBoost;
        private PowerupEffect _activePowerup = null;
        private Sprite _activePowerupSprite;
        private Node2D _activePowerupNode;
        private Area2D _powerupArea;
        private int _fishStreak = 0;
        private float _fishStreakTimeElapsed = 0;
        private float _fishBoostTimeRemaining = 0;
        private AudioStreamPlayer _obstaclesDestroyedAudio;
        private AudioStreamPlayer _fishBoostAudio;
        private Trail2D _boostTrail;
        private ViewportContainer _orcaSprites;
        private Hat _hat;
        private SceneTreeTween _speedBoostTween;

        private enum CollisionState
        {
            NONE = 0, CEILING = 1, FLOOR = 2
        }

        public override void _Ready()
        {
            var viewport = GetNode<Viewport>("OrcaSprites/Viewport");
            viewport.SetFilter(isFilterEnabled: true);
            _orcaSprites = GetNode<ViewportContainer>("OrcaSprites");
            _orcaMouth = GetNode<AnimatedSprite>("OrcaSprites/Viewport/OrcaMouthAnimation");
            _orcaBody = GetNode<AnimatedSprite>("OrcaSprites/Viewport/OrcaAnimation");
            _finlet1 = GetNode<Node2D>("Finlet1");
            _finlet2 = GetNode<Node2D>("Finlet2");
            _effectsPlayer = GetNode<AnimationPlayer>("EffectsPlayer");
            _bubbleEmitter = GetNode<OrcaBubbleEmitter>("TransformInheritanceBlocker/OrcaPositionReceiver/OrcaBubbleEmitter");
            _speedBoost = GetNode<Node2D>("SpeedBoost");
            _obstaclesDestroyedAudio = GetNode<AudioStreamPlayer>("ObstaclesDestroyedAudio");
            _fishBoostAudio = GetNode<AudioStreamPlayer>("FishBoostAudio");
            _boostTrail = GetNode<Trail2D>("TrailEmissionPoint/BoostTrail");
            _dialogueSource = GetNode<Node2D>("DialogueSource");
            _hat = GetNode<Hat>("Hat");

            _activePowerupSprite = GetNode<Sprite>("ActivePowerup/ActivePowerupNode/ActivePowerupSprite");
            _activePowerupSprite.Visible = false;
            _activePowerupNode = GetNode<Node2D>("ActivePowerup/ActivePowerupNode");
            _powerupArea = GetNode<Area2D>("ActivePowerup/ActivePowerupNode/PowerupEffectArea");
            _powerupArea.Connect(BuiltInSignals.AreaEntered, this, nameof(OnPowerupAreaEntered));
            var powerupAnimationPlayer = GetNode<AnimationPlayer>("PowerupAnimationPlayer");

            RecalculateAndUpdateBoostTrail();

            Velocity = new Vector2(HorizontalSpeed, 0f);
            _remainingInvulnerabilityTime = 0f;
            _collisionState = CollisionState.NONE;
            UpdateHealth(StartingHealth);
            _nearbyConsumablesCount = 0;
            _bubbleEmitter.OnBiomeUpdated(Settings.StartingBiome);
            _hat.ActiveHat = ActiveHat;

            if (!IsPlaceholder)
            {
                GlobalSignalBus.RegisterEmitter(Signals.DisplayedStatChanged, this);
                GlobalSignalBus.RegisterEmitter(Signals.OrcaHealthChanged, this);

                GlobalSignalBus.RegisterHandler(Signals.HazardTriggered, this, nameof(OnHazardTriggered));
                GlobalSignalBus.RegisterHandler(Signals.PowerupTriggered, this, nameof(OnPowerupTriggered));
                GlobalSignalBus.RegisterHandler(Signals.PrizeTriggered, this, nameof(OnPrizeTriggered));
                GlobalSignalBus.RegisterHandler(Signals.OrcaEnteredProximity, this, nameof(OnOrcaEnteredProximity));
                GlobalSignalBus.RegisterHandler(Signals.OrcaExitedProximity, this, nameof(OnOrcaExitedProximity));
                GlobalSignalBus.RegisterHandler(Signals.BiomeChanged, this, nameof(OnBiomeUpdated));
                GlobalSignalBus.RegisterHandler(Signals.GameOver, this, nameof(OnGameOver));
            }

            LinkOrcaTransformToFollower();
        }

        private void OnGameOver()
        {
            QueueFree();
        }

        public bool IsCurrentlyInvulnerable()
        {
            return _remainingInvulnerabilityTime > 0 || IsCurrentlyDestroyingObstacles();
        }

        public bool IsCurrentlyDestroyingObstacles()
        {
            return _activePowerup?.Type == PowerupEnum.Shield || IsBoostActive();
        }

        private void LinkOrcaTransformToFollower()
        {
            if (PositionFollower != null)
            {
                _remoteTransform = new RemoteTransform2D()
                {
                    UpdateRotation = false,
                    UpdateScale = false,
                    RemotePath = "../" + PositionFollower // Need to modify the nodepath, because the path for PositionFollower is relative to Orca, while the remote transform needs a path relative to itself 
                };
                AddChild(_remoteTransform);
            }
        }

        public void OnHazardTriggered(CollectibleHazard hazard)
        {
            if (!IsCurrentlyInvulnerable())
            {
                UpdateHealth(_health - hazard.Damage);
                hazard.ShouldPlayOnCollectedEffects = true;
                hazard.ShouldEmitParticles = false;
            }
            else
            {
                hazard.ShouldPlayOnCollectedEffects = false;

                if (IsCurrentlyDestroyingObstacles())
                {
                    hazard.ShouldEmitParticles = true;
                    ProgressTracker.Progress.CurrentSessionStats.IncrementStat(StatEnum.DestroyedObstacle);
                    if (hazard.DestroyedObstacleStatType != StatEnum.Default)
                    {
                        ProgressTracker.Progress.CurrentSessionStats.IncrementStat(hazard.DestroyedObstacleStatType);
                    }

                    EmitSignal(Signals.DisplayedStatChanged);
                    GDUtils.VibrateIfAble(25);
                    _obstaclesDestroyedAudio.PlayIfAble();
                }
                else
                {
                    hazard.ShouldEmitParticles = false;
                }
            }
        }

        public void OnPrizeTriggered(CollectiblePrize prize)
        {
            if (_fishStreak < FishBoostThreshold)
            {
                _fishStreak += 1;
            }

            _fishStreakTimeElapsed = 0;

            prize.PitchScale = BasePrizePitch + (_fishStreak * .05f);

            if (!IsBoostActive())
            {
                if (_fishStreak == FishBoostThreshold)
                {
                    StartFishBoost();
                    prize.PitchScale = 0f;
                }
                else
                {
                    RecalculateAndUpdateBoostTrail();
                }
            }
        }

        public void StartFishBoost()
        {
            _fishStreak = FishBoostThreshold;
            _fishBoostAudio.PlayIfAble();
            bool boostWasAlreadyActive = IsBoostActive();

            ProgressTracker.Progress.CurrentSessionStats.IncrementStat(StatEnum.FishBoost);
            _fishBoostTimeRemaining = FishBoostDuration;

            _speedBoost.Visible = true;
            _speedBoost.Modulate = new Color(1f, 1f, 1f, 1f);
            
            _speedBoostTween?.Stop();
            _speedBoostTween = CreateTween();
            _speedBoostTween.TweenInterval(FishBoostDuration);
            _speedBoostTween.TweenProperty(_speedBoost, "modulate", new Color(1f, 1f, 1f, 0f), FishBoostGracePeriod);
            
            RecalculateAndUpdateBoostTrail();
            if (!boostWasAlreadyActive)
            {
                var newXSpeed = Velocity.x + FishBoostXSpeedIncrease;
                Velocity = new Vector2(newXSpeed, Velocity.y);
            }
        }

        public bool IsBoostActive()
        {
            return _fishBoostTimeRemaining > -FishBoostGracePeriod;
        }

        public void IncreaseHorizontalSpeed(float horizontalSpeedIncrease)
        {
            Velocity.x += horizontalSpeedIncrease;
        }

        public void OnPowerupDeactivated()
        {
            switch (_activePowerup.Type)
            {
                case PowerupEnum.Ghost:
                    var normalModulateColor = new Color(1f, 1f, 1f, 1f);
                    _orcaSprites.SelfModulate = normalModulateColor;
                    _finlet1.Modulate = normalModulateColor;
                    _finlet2.Modulate = normalModulateColor;
                    _hat.Modulate = normalModulateColor;
                    SetCollisionLayerBit(CollisionLayers.HazardLayer, true);
                    SetCollisionMaskBit(CollisionLayers.HazardLayer, true);
                    break;
                default:
                    break;
            }
        }

        public void OnPowerupTriggered(PowerupEffect powerup)
        {
            if (powerup.Duration != 0)
            {
                if (_activePowerup != null)
                {
                    OnPowerupDeactivated();
                }

                _activePowerup = powerup;
                _activePowerupSprite.Visible = true;
                _activePowerupNode.Scale = new Vector2(powerup.Scale, powerup.Scale);
                _activePowerupSprite.Modulate = powerup.ModulateColor;

                var areasAlreadyInPowerupRange = _powerupArea.GetOverlappingAreas().Cast<Area2D>();
                foreach (var area in areasAlreadyInPowerupRange)
                {
                    OnPowerupAreaEntered(area);
                }
            }

            switch (powerup.Type)
            {
                case PowerupEnum.HealthPack:
                    UpdateHealth(_health + 1);
                    break;
                case PowerupEnum.Ghost:
                    var semiTransparentColor = new Color(1f, 1f, 1f, .5f);
                    _orcaSprites.SelfModulate = semiTransparentColor;
                    _finlet1.Modulate = semiTransparentColor;
                    _finlet2.Modulate = semiTransparentColor;
                    _hat.Modulate = semiTransparentColor;
                    SetCollisionLayerBit(CollisionLayers.HazardLayer, false);
                    SetCollisionMaskBit(CollisionLayers.HazardLayer, false);
                    break;
                default:
                    break;
            }
        }

        public void OnPowerupAreaEntered(Node nearbyObject)
        {
            if (_activePowerup?.Type == PowerupEnum.Magnet && _activePowerup.IsActive && nearbyObject is CollectiblePrize)
            {
                var prize = nearbyObject as CollectiblePrize;
                prize.MagnetizingBody = this;
            }
            else if (_activePowerup?.Type == PowerupEnum.Shield && _activePowerup.IsActive && nearbyObject is CollectibleHazard)
            {
                var hazard = nearbyObject as CollectibleHazard;
                hazard.GetCollected(this);
            }
        }

        public void OnOrcaEnteredProximity(Node nearbyObject)
        {
            if (nearbyObject is CollectiblePrize || nearbyObject.IsInGroup(Groups.TransitionSegment))
            {
                _nearbyConsumablesCount++;
                UpdateMouthBasedOnNearbyConsumables();
            }
        }

        public void OnOrcaExitedProximity(Node nearbyObject)
        {
            if (nearbyObject is CollectiblePrize || nearbyObject.IsInGroup(Groups.TransitionSegment))
            {
                _nearbyConsumablesCount--;
                UpdateMouthBasedOnNearbyConsumables();
            }
        }

        private void UpdateMouthBasedOnNearbyConsumables()
        {
            const int openMouthFrame = 1;
            const int closedMouthFrame = 0;
            if (_nearbyConsumablesCount > 0)
            {
                _orcaMouth.Frame = openMouthFrame;
            }
            else
            {
                _orcaMouth.Frame = closedMouthFrame;
            }
        }

        public void UpdateHealth(int newHealth)
        {
            if (newHealth > StartingHealth)
            {
                // cannot gain more HP than max/starting HP
                return;
            }

            if (newHealth < _health)
            {
                // then, we were damaged
                _hasTakenDamageInCurrentBiome = true;
                _fishStreak = 0;
                RecalculateAndUpdateBoostTrail();
                _remainingInvulnerabilityTime = InvulnerabilityTimeAfterDamaged;
                _effectsPlayer.Play(Animations.OrcaDamaged);
                if (newHealth > 0)
                {
                    GDUtils.VibrateIfAble(50);
                }
                else
                {
                    GDUtils.VibrateIfAble(100);
                }

            }

            _health = newHealth;
            EmitSignal(nameof(OrcaHealthChanged), _health);

            SetFinletVisibilitiesBasedOnCurrentHealth();
        }

        private void SetFinletVisibilitiesBasedOnCurrentHealth()
        {
            switch (_health)
            {
                case 3:
                    _finlet1.Visible = true;
                    _finlet2.Visible = true;
                    break;
                case 2:
                    _finlet1.Visible = true;
                    _finlet2.Visible = false;
                    break;
                case 1:
                case 0:
                    _finlet1.Visible = false;
                    _finlet2.Visible = false;
                    break;
                default:
                    GD.PrintErr("Unexpected health value for Orca.");
                    break;
            }
        }

        private void RecalculateAndUpdateBoostTrail()
        {
            float progressTowardsBoost = (float)_fishStreak / FishBoostThreshold;
            int segmentsToAdd = (int)(BoostTrailSegmentRange * progressTowardsBoost);

            var targetTrailLength = BoostTrailMinSegments + segmentsToAdd;
            Color trailColor = IsBoostActive() ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, .6f);
            float width = IsBoostActive() ? 2.8f : 1.5f;

            var tween = CreateTween();
            tween.TweenProperty(_boostTrail, "trailLength", targetTrailLength, FishBoostGracePeriod);
            tween.Parallel().TweenProperty(_boostTrail, "modulate", trailColor, FishBoostGracePeriod);
            tween.Parallel().TweenProperty(_boostTrail, "width", width, FishBoostGracePeriod);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            _fishStreakTimeElapsed += delta;
            if (_fishStreakTimeElapsed > FishStreakMaxTimeDelay && _fishStreak > 0)
            {
                _fishStreak = 0;
                if (!IsBoostActive())
                {
                    RecalculateAndUpdateBoostTrail();
                }
            }

            if (_activePowerup != null)
            {
                _activePowerup.DurationElapsed += delta;
                if (!_activePowerup.IsActive)
                {
                    OnPowerupDeactivated();
                    _activePowerup = null;
                    _activePowerupSprite.Visible = false;
                }
            }

            _fishBoostTimeRemaining -= delta;
            if (_fishBoostTimeRemaining <= -FishBoostGracePeriod)
            {
                if (_speedBoost.Visible)
                {
                    _speedBoost.Visible = false;
                    var newXSpeed = Velocity.x - FishBoostXSpeedIncrease;
                    Velocity = new Vector2(newXSpeed, Velocity.y);
                    _fishStreak = 0;
                    RecalculateAndUpdateBoostTrail();
                }
            }

            bool wasPreviouslyInvulnerable = _remainingInvulnerabilityTime > 0f;
            _remainingInvulnerabilityTime -= delta;
            if (_remainingInvulnerabilityTime <= 0f && wasPreviouslyInvulnerable)
            {
                _effectsPlayer.ActuallyStop(reset: true);
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            var acceleration = new Vector2(0, VerticalAccelerationOnPress);

            if (Input.IsActionJustPressed("dive") || Input.IsActionJustReleased("dive"))
            {
                GDUtils.VibrateIfAble(20);
            }

            if (Input.IsActionPressed("dive"))
            {
                if (_collisionState == CollisionState.CEILING)
                {
                    _collisionState = CollisionState.NONE;
                }

                Velocity += acceleration;
                if (Rotation < MaxRotationRadians && _collisionState != CollisionState.FLOOR)
                {
                    Rotation += RotationSpeedRadians;
                }
            }
            else
            {
                if (_collisionState == CollisionState.FLOOR)
                {
                    _collisionState = CollisionState.NONE;
                }

                Velocity -= acceleration;
                if (Rotation > -MaxRotationRadians && _collisionState != CollisionState.CEILING)
                {
                    Rotation -= RotationSpeedRadians;
                }
            }

            if (_collisionState == CollisionState.CEILING)
            {
                Rotation = Math.Min(Rotation + RotationSpeedRadians * .75f, 0);
            }
            else if (_collisionState == CollisionState.FLOOR)
            {
                Rotation = Math.Max(Rotation - RotationSpeedRadians * .75f, 0);
            }

            if (Velocity.y > MaxVerticalSpeed)
            {
                Velocity = new Vector2(Velocity.x, MaxVerticalSpeed);
            }
            else if (Velocity.y < -MaxVerticalSpeed)
            {
                Velocity = new Vector2(Velocity.x, -MaxVerticalSpeed);
            }

            var velocityAfterSliding = MoveAndSlide(Velocity);
            Velocity.y = velocityAfterSliding.y;

            var firstCollision = GetSlideCount() > 0 ? GetSlideCollision(0) : null;
            bool orcaHitAnInvisibleWall = firstCollision != null && firstCollision.Collider is InvisibleWall;
            if (orcaHitAnInvisibleWall)
            {
                var collidingWall = firstCollision.Collider as InvisibleWall;
                bool collidingWallIsCeiling = collidingWall.Position.y < Position.y;
                if (collidingWallIsCeiling)
                {
                    _collisionState = CollisionState.CEILING;
                }
                else
                {
                    _collisionState = CollisionState.FLOOR;
                }

                Velocity.y = 0;
            }

            if (ProgressTracker.Progress != null)
            {
                ProgressTracker.Progress.CurrentSessionStats[StatEnum.Distance] = DistanceTraveledMeters;

                EmitSignal(Signals.DisplayedStatChanged);
            }
        }

        public void OnBiomeUpdated(BiomeEnum biome)
        {
            if (!_hasTakenDamageInCurrentBiome)
            {
                ProgressTracker.Progress.CurrentSessionStats.IncrementStat(StatEnum.DamageFreeBiomes);
            }
            else
            {
                _hasTakenDamageInCurrentBiome = false;
            }
        }

        public int DistanceTraveledMeters { get { return (int)Position.x / UnitsPerMeter; } }
    }
}
