using Godot;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Orcas;

namespace Orcinus.Scripts.CollectibleObjects
{
    public class CollectibleYachtPrize : CollectiblePrize
    {
        public override PhysicsBody2D MagnetizingBody { get => null; set { } }

        private AnimationPlayer _yachtAnimator;

        public static bool YachtHasSpawned = false;

        public override void _Ready()
        {
            base._Ready();
            _yachtAnimator = GetNode<AnimationPlayer>("YachtAnimator");

            if (YachtHasSpawned && !_isDespawningOnCreation)
            {
                GD.Print("Yacht spawn was cancelled, since a yacht has already been spawned during this run.");

                this.QueueFree();
            }
            else if (!_isDespawningOnCreation)
            {
                GD.Print("Yacht successfully spawned");
                YachtHasSpawned = true;
            }
        }

        protected override void OnCollected(Orca orca)
        {
            SteamWrapper.UnlockAchievement(AchievementEnum.Prankster);
            _yachtAnimator.Play("sink_yacht");
            orca.StartFishBoost();
            Velocity = 0;
            base.OnCollected(orca);
        }
    }
}
