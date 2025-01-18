using Godot;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public class CollectiblesTerminator : CollectiblesDetector
    {
        public override void OnCollectibleDetected(CollectibleObject obj)
        {
            obj.OnDestroyedWithoutCollecting();

            obj.QueueFree();
        }

        public override void OnLevelSegmentEndDetected(Area2D levelSegmentEnd)
        {
            if (!levelSegmentEnd.IsInGroup(Groups.BiomeTransitionMarker))
            {
                levelSegmentEnd.GetParent().QueueFree();
            }
        }
    }
}