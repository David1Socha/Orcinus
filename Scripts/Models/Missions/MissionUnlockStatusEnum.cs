namespace Orcinus.Scripts.Models.Missions
{
    public enum MissionUnlockStatusEnum
    {
        LOCKED = 0,
        IN_PROGRESS = 1,
        UNLOCKED_VIA_MISSIONS = 2,
        UNLOCKED_VIA_POINTS = 3, // these values might not all be needed. still thinking through this whole system
        UNLOCKED_VIA_FISH = 4,
    }
}
