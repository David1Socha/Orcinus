using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Orcas;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public class TransitionSegment : Node2D
    {
        private Sprite _net;
        private Particles2D _onReleaseParticles;
        private AudioStreamPlayer _onFreedAudio;
        private AbstractOrca _abstractOrca;
        public BiomeEnum AssociatedBiome { get; set; }

        [Export]
        public bool IsTutorial { get; set; }

        [Signal]
        public delegate void OrcaFreed();

        public override void _Ready()
        {
            base._Ready();
            GlobalSignalBus.RegisterEmitter(Signals.OrcaFreed, this);
            _net = GetNode<Sprite>("OrcaNet");
            _onReleaseParticles = GetNode<Particles2D>("OnOrcaReleaseParticles");
            _onFreedAudio = GetNode<AudioStreamPlayer>("OnFreedAudio");
            _abstractOrca = GetNode<AbstractOrca>("AbstractOrca");
            if (IsTutorial)
            {
                var biomeTransition = GetNode<Node2D>("BiomeTransition");
                biomeTransition.QueueFree();
                _abstractOrca.QueueFree();
            }
            else
            {
                _abstractOrca.AssociatedBiome = AssociatedBiome;
            }
        }

        public void OnBodyEnteredMe(Node enteringBody)
        {
            if (enteringBody is Orca)
            {
                _net.Visible = false;
                _onReleaseParticles.Emitting = true;
                EmitSignal(Signals.OrcaFreed);
                _onFreedAudio.Play();
            }
        }
    }
}
