using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Orcas;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectiblePowerup : CollectibleObject
    {
        [Export]
        public PowerupEnum Type { get; set; }
        [Export]
        public float Duration { get; set; }
        [Export]
        public Color ModulateColor { get; set; }
        [Export]
        public Texture PowerupIcon { get; set; }
        [Export]
        public float PowerupScale { get; set; }

        private Sprite _powerupBubble;

        public override void _Ready()
        {
            base._Ready();
            _powerupBubble = GetNode<Sprite>("PowerupBubble");

            GlobalSignalBus.RegisterEmitter(Signals.PowerupTriggered, this);
        }

        protected override void OnCollected(Orca orca)
        {
            base.OnCollected(orca);

            EmitSignal(Signals.PowerupTriggered, new PowerupEffect { Duration = Duration, Type = Type, ModulateColor = ModulateColor, PowerupIcon = PowerupIcon, Scale = PowerupScale });

            _powerupBubble.Visible = false;
        }
    }
}
