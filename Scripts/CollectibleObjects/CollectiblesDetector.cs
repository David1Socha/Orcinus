using Godot;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.CollectibleObjects
{
    public abstract class CollectiblesDetector : Area2D
    {
        public void OnAreaEnteredMe(Area2D enteringArea)
        {
            if (enteringArea is CollectibleObject)
            {
                var collectible = enteringArea as CollectibleObject;
                OnCollectibleDetected(collectible);
            }
            else if (enteringArea.IsInGroup(Groups.LevelSegmentEnd))
            {
                OnLevelSegmentEndDetected(enteringArea);
            }
        }

        public abstract void OnLevelSegmentEndDetected(Area2D levelSegmentEnd);
        public abstract void OnCollectibleDetected(CollectibleObject obj);
    }
}
