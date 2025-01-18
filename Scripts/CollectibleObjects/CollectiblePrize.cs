using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Orcas;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectiblePrize : CollectibleObject, IDialogueSource
    {
        private Node2D _dialogueSourceNode;
        public Vector2 DialogueScreenPosition => _dialogueSourceNode.GetGlobalTransformWithCanvas().origin;

        public string DialogueName { get; set; }

        public Color? DialogueColor => null;

        public override void _Ready()
        {
            base._Ready();
            _dialogueSourceNode = GetNode<Node2D>("DialogueSource");

            GlobalSignalBus.RegisterEmitter(Signals.PrizeTriggered, this);
        }

        protected override void OnCollected(Orca orca)
        {
            EmitSignal(Signals.PrizeTriggered, this);

            base.OnCollected(orca);
        }
    }
}
