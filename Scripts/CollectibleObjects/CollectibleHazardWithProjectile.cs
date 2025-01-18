using Godot;
using Orcinus.Scripts.Core;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectibleHazardWithProjectile : CollectibleHazard
    {
        [Export]
        public readonly PackedScene ProjectileScene;
        [Export]
        public readonly float RetreatVelocity;
        [Export]
        public readonly Texture RetreatTexture;
        [Export]
        public readonly Vector2[] ProjectileDirections;
        private AudioStreamPlayer _onProjectileFiredAudio;
        protected Node2D _spawnPoint;
        protected Timer _projectileTimer;

        public override void _Ready()
        {
            _spawnPoint = GetNode<Node2D>("ProjectileSpawnPoint");
            _projectileTimer = GetNode<Timer>("ProjectileTimer");
            _onProjectileFiredAudio = GetNode<AudioStreamPlayer>("OnProjectileFiredAudio");

            base._Ready();
            _animation.Animation = Animations.Default;

            if (_animation?.Frames != null && !_animation.Frames.HasAnimation("retreat"))
            {
                GD.PrintErr("Warning - no 'retreat' animation defined for CollectibleHazardWithProjectile - " + Name);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _projectileTimer.Paused = false;
        }

        public override void Disable()
        {
            base.Disable();
            _projectileTimer.Paused = true;
        }

        public void OnProjectileTimerTimeout()
        {
            var projectile = ProjectileScene.Instance<CollectibleObject>();
            projectile.DirectionVector = ProjectileDirections.PickRandomElement(_random);
            GetParent().AddChildDeferred(projectile);
            projectile.SetDeferred("global_position", GlobalPosition);
            projectile.SetDeferred("rotation", Mathf.Atan2(projectile.DirectionVector.y, projectile.DirectionVector.x));

            _onProjectileFiredAudio.PlayIfAble();

            Velocity = RetreatVelocity;
            if (RetreatTexture != null)
            {
                _sprite.Texture = RetreatTexture;
            }

            if (_animation?.Frames != null)
            {
                _animation.Animation = Animations.Retreat;
            }
        }
    }
}
