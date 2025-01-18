using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Orcas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectibleObject : Area2D
    {
        [Export]
        public readonly int FishValue;

        [Export]
        public readonly StatEnum OnCollectedStatType = StatEnum.Default;

        [Export]
        public readonly StatEnum OnDestroyedStatType = StatEnum.Default;

        [Export]
        public readonly StatEnum OnNearMissedStatType = StatEnum.Default;

        [Export]
        public float Velocity;

        [Export]
        public NodePath[] NodesToHideOnCollect { get; set; }

        [Export]
        public Vector2 DirectionVector { get; set; }

        [Export]
        public readonly bool DisabledOnLoad = true;

        [Export]
        public Vector2 PositionVariance { get; set; }

        [Export(PropertyHint.Range, "0,1,.01")]
        public float SpawnProbability { get; set; } = 1f;

        [Export]
        public bool DeleteOnCollection { get; set; } = true;

        [Export]
        public NodePath[] AnimationPlayersToRandomize { get; set; }

        [Signal]
        public delegate void HazardTriggered(CollectibleHazard hazard);

        [Signal]
        public delegate void PowerupTriggered(CollectiblePowerup powerup);

        [Signal]
        public delegate void PrizeTriggered(CollectiblePrize prize);

        public virtual PhysicsBody2D MagnetizingBody { get; set; }

        public bool OrcaHasBeenNearby { get; set; }
        public bool ShouldEmitParticles { get; set; } = true;
        public bool ShouldPlayOnCollectedEffects { get; set; } = true;

        private AudioStreamPlayer _onCollectedAudio;
        protected AnimatedSprite _animation;
        protected Sprite _sprite, _backgroundAura;
        protected RandomNumberGenerator _random;
        private AnimatedSprite _onCollectedAnimation;
        protected Particles2D _onCollectedParticles;
        protected bool _isDespawningOnCreation = false;
        public float PitchScale { get; set; } = 1;

        public override void _Ready()
        {
            base._Ready();
            _random = new RandomNumberGenerator();
            _random.Randomize();
            _onCollectedAudio = GetNode<AudioStreamPlayer>("OnCollectedAudio");
            _animation = GetNode<AnimatedSprite>("CollectibleObjectAnimation");
            _sprite = GetNode<Sprite>("CollectibleObjectSprite");
            _onCollectedAnimation = GetNode<AnimatedSprite>("OnCollectedAnimation");
            _backgroundAura = GetNode<Sprite>("BackgroundAura");
            _onCollectedParticles = GetNode<Particles2D>("OnCollectedParticles");

            if (SpawnProbability < 1f)
            {
                
                float randomDraw = _random.Randf();
                if (randomDraw > SpawnProbability)
                {
                    _isDespawningOnCreation = true;
                    GD.Print($"{this.Name} was despawned on creation because random draw of {randomDraw} was greater than spawn probability of {SpawnProbability}");
                    this.QueueFree();
                    return;
                }
            }

            var currentAnim = _animation.SetAnimationToRandomValue();
            if (currentAnim != null)
            {
                // set animation speed and current frame to be somewhat random - so that multiple nearby objects of the same type don't look too identical
                _animation.SpeedScale = _random.RandfRange(.9f, 1.1f);
                _animation.Frame = _random.RandiRange(0, _animation.Frames.GetFrameCount(currentAnim));
            }

            foreach (var animationNodePath in AnimationPlayersToRandomize)
            {
                var animationPlayer = GetNode<AnimationPlayer>(animationNodePath);
                animationPlayer.Advance(_random.RandfRange(0f, animationPlayer.CurrentAnimationLength));
            }

            if (PositionVariance != Vector2.Zero)
            {
                var xPosOffset = _random.RandfRange(-PositionVariance.x, PositionVariance.x);
                var yPosOffset = _random.RandfRange(-PositionVariance.y, PositionVariance.y);
                Position += new Vector2(xPosOffset, yPosOffset);
            }

            // To simplify level structuring and reduce processing demands, most objects start out as disabled. They become enabled shortly before they enter the player's view.
            if (DisabledOnLoad)
            {
                Disable();
            }
        }

        public virtual void Disable()
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        public virtual void Enable()
        {
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public virtual void OnOrcaEnteredProximity(CollectibleObject me)
        {
            OrcaHasBeenNearby = true;
        }

        public virtual void OnOrcaExitedProximity(CollectibleObject me)
        {

        }

        public virtual void OnDestroyedWithoutCollecting()
        {
            if (this is CollectibleHazard)
            {
                ProgressTracker.Progress.CurrentSessionStats.IncrementStat(StatEnum.DodgedObstacle);
                if (OrcaHasBeenNearby)
                {
                    ProgressTracker.Progress.CurrentSessionStats.IncrementStat(StatEnum.NearMissedObstacle);
                }
            }

            if (OnDestroyedStatType != StatEnum.Default)
            {
                ProgressTracker.Progress.CurrentSessionStats.IncrementStat(OnDestroyedStatType);
            }

            if (OnNearMissedStatType != StatEnum.Default && OrcaHasBeenNearby)
            {
                ProgressTracker.Progress.CurrentSessionStats.IncrementStat(OnNearMissedStatType);
            }
        }

        public void OnBodyEnteredMe(Node enteringBody)
        {
            if (enteringBody is Orca)
            {
                GetCollected(enteringBody as Orca);
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (MagnetizingBody != null)
            {
                GlobalPosition = GlobalPosition.MoveToward(MagnetizingBody.GlobalPosition, 1300 * delta);
            }
            else if (Velocity != default)
            {
                var movement = Velocity * delta * DirectionVector.Normalized();
                Position += movement;
            }
        }

        protected virtual void OnCollected(Orca orca)
        {
        }

        public async void GetCollected(Orca orca)
        {
            OnCollected(orca);

            if (OnCollectedStatType != StatEnum.Default)
            {
                ProgressTracker.Progress.CurrentSessionStats.IncrementStat(OnCollectedStatType);
            }


            await BeginCollectibleObjectDeletion();
        }

        private async Task BeginCollectibleObjectDeletion()
        {
            var preDeletionTasks = new List<Task>();

            if (ShouldPlayOnCollectedEffects)
            {
                preDeletionTasks.Add(this.WaitForAudioCompletion(_onCollectedAudio));
                if (PitchScale > 0f)
                {
                    _onCollectedAudio.PitchScale = PitchScale;
                    _onCollectedAudio.PlayIfAble();
                }
            }

            _sprite.Visible = false;
            _animation.Visible = false;
            _backgroundAura.Visible = false;
            foreach (var path in NodesToHideOnCollect)
            {
                var node = GetNode<Node2D>(path);
                node.Visible = false;
            }

            if (ShouldEmitParticles)
            {
                _onCollectedParticles.Emitting = true;
                preDeletionTasks.Add(GDUtils.WaitAsync(this, _onCollectedParticles.Lifetime));
            }

            SetDeferred("monitorable", false);
            SetDeferred("monitoring", false);

            if (_onCollectedAnimation?.Frames != null && ShouldPlayOnCollectedEffects)
            {
                _onCollectedAnimation.Visible = true;
                preDeletionTasks.Add(this.WaitForAnimationCompletion(_onCollectedAnimation));
                _onCollectedAnimation.Play();
            }

            await Task.WhenAll(preDeletionTasks);

            if (DeleteOnCollection)
            {
                Visible = false;
                QueueFree();
            }
            
        }
    }
}
