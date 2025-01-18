using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Orcas;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectibleHazard : CollectibleObject
    {
        [Export]
        public readonly int Damage;

        [Export]
        public readonly StatEnum DestroyedObstacleStatType;

        public override void _Ready()
        {
            base._Ready();

            GlobalSignalBus.RegisterEmitter(Signals.HazardTriggered, this);
        }

        protected override void OnCollected(Orca orca)
        {
            EmitSignal(Signals.HazardTriggered, this);
        }
    }
}
