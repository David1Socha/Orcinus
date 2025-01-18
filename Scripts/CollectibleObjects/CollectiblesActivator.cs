using Godot;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public class CollectiblesActivator : CollectiblesDetector
    {
        [Signal]
        public delegate void SegmentEndReached(float segmentEndXPosition);
        [Signal]
        public delegate void BiomeTransitionReached();

        public override void _Ready()
        {
            base._Ready();
        }

        public override void OnCollectibleDetected(CollectibleObject obj)
        {
            obj.Enable();
        }

        public override void OnLevelSegmentEndDetected(Area2D levelSegmentEnd)
        {
            float endOfSegmentXPosition = levelSegmentEnd.GlobalPosition.x + 100f; // default 100f padding between segments
            var isBiomeTransition = levelSegmentEnd.IsInGroup(Groups.BiomeTransitionMarker);
            if (isBiomeTransition)
            {
                EmitSignal(nameof(BiomeTransitionReached));
            }
            else
            {
                EmitSignal(nameof(SegmentEndReached), endOfSegmentXPosition);
            }

        }
    }
}