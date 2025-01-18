using Newtonsoft.Json;

namespace Orcinus.Scripts.Models.Missions
{
    public class Mission
    {
        [JsonIgnore]
        public int AssociatedLevel { get { return (MissionId / 3) + 1; } }
        [JsonIgnore]
        public int MissionId { get; set; }
        public MissionUnlockStatusEnum Status { get; set; }
        [JsonIgnore]
        public StatEnum AssociatedStat { get; set; }
        [JsonIgnore]
        public OrcaEnum? AssociatedOrca { get; set; }
        [JsonIgnore]
        public string MissionDescription { get; set; }
        public int MissionProgress { get; set; }
        [JsonIgnore]
        public int MissionThreshold { get; set; }
        [JsonIgnore]
        public bool IsSingleSession { get; set; }
        [JsonIgnore]
        public bool MissionWasJustUnlocked { get; set; }

        [JsonIgnore]
        public bool IsUnlocked
        {
            get
            {
                return Status == MissionUnlockStatusEnum.UNLOCKED_VIA_FISH || Status == MissionUnlockStatusEnum.UNLOCKED_VIA_MISSIONS || Status == MissionUnlockStatusEnum.UNLOCKED_VIA_POINTS;
            }
        }

        public bool RecalculateCompletionStatus(Progress progress)
        {
            var missionWasJustUnlocked = false;
            if (Status == MissionUnlockStatusEnum.IN_PROGRESS)
            {
                var isMissionCompleted = MissionProgress >= MissionThreshold;
                if (isMissionCompleted)
                {
                    Status = MissionUnlockStatusEnum.UNLOCKED_VIA_MISSIONS;
                    missionWasJustUnlocked = true;
                }
            }

            MissionWasJustUnlocked = missionWasJustUnlocked;
            return missionWasJustUnlocked;
        }

        public bool IsPendingUnlock(Progress progress)
        {
            bool missionProgressExceedsThreshold = MissionProgress + GetCurrentMissionProgress(progress) >= MissionThreshold;
            if (missionProgressExceedsThreshold && !IsUnlocked)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetCurrentMissionProgress(Progress progress)
        {
            if (AssociatedOrca == null || AssociatedOrca == Settings.ActiveOrca)
            {
                if (progress.CurrentSessionStats == null)
                {
                    return 0;
                }
                else
                {
                    var missionProgress = progress.CurrentSessionStats[AssociatedStat];

                    return missionProgress;
                }
            }
            else
            {
                return 0;
            }
        }

    }
}
