using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Orcas;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.ProximityDetectors
{
    public class ProximityDetector : Area2D
    {
        [Signal]
        public delegate void OrcaEnteredProximity(Node nearbyObject);

        [Signal]
        public delegate void OrcaExitedProximity(Node nearbyObject);

        public override void _Ready()
        {
            base._Ready();

            GlobalSignalBus.RegisterEmitter(Signals.OrcaExitedProximity, this);
            GlobalSignalBus.RegisterEmitter(Signals.OrcaEnteredProximity, this);
        }

        public void OnAreaEnteredMe(Node enteringBody)
        {
            if (enteringBody is OrcaMouthCollision)
            {
                EmitSignal(nameof(OrcaEnteredProximity), GetParent());
            }
        }

        public void OnAreaExitedMe(Node exitingBody)
        {
            if (exitingBody is OrcaMouthCollision)
            {
                EmitSignal(nameof(OrcaExitedProximity), GetParent());
            }
        }
    }
}
