using Godot;
using Newtonsoft.Json;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models.Missions;
using System.Collections.Generic;
using System.Linq;

namespace Orcinus.Scripts.Models
{
    public class ProgressTracker
    {
        public static Progress Progress { get; set; }
        private const string ProgressFileLocation = "user://orcinus_progress.json";
        private static readonly object ProgressFileLock = new object();
        public const int ActiveMissionCount = 3;

        [Signal]
        public delegate void NotificationQueued();

        private static void AddTemplateMissionValuesIntoSavedMission(Mission savedMission, Mission templateMission)
        {
            savedMission.IsSingleSession = templateMission.IsSingleSession;
            savedMission.MissionDescription = templateMission.MissionDescription;
            savedMission.AssociatedStat = templateMission.AssociatedStat;
            savedMission.AssociatedOrca = templateMission.AssociatedOrca;
            savedMission.MissionThreshold = templateMission.MissionThreshold;
            savedMission.MissionId = templateMission.MissionId;
        }

        public static Progress LoadProgress()
        {
            lock (ProgressFileLock)
            {
                using (var progressFile = new File())
                {
                    if (progressFile.FileExists(ProgressFileLocation))
                    {
                        progressFile.Open(ProgressFileLocation, File.ModeFlags.Read);
                        string progressTextToLoad = progressFile.GetAsText();
                        Progress = JsonConvert.DeserializeObject<Progress>(progressTextToLoad);
                        var defaultMissions = MissionDefinitions.CreateDefaultMissionList();
                        var missions = defaultMissions.Select(templateMission =>
                        {
                            var mId = templateMission.MissionId;
                            bool hasSavedMission = Progress.Missions.Count > mId;
                            if (hasSavedMission)
                            {
                                var savedMission = Progress.Missions[mId];
                                AddTemplateMissionValuesIntoSavedMission(savedMission, templateMission);
                                return savedMission;
                            }
                            else
                            {
                                return templateMission;
                            }
                        });
                        Progress.Missions = missions.ToList();
                        var inProgressMissions = Progress.Missions.Where(m => m.Status == MissionUnlockStatusEnum.IN_PROGRESS);
                        var lockedMissions = Progress.Missions.Where(m => m.Status == MissionUnlockStatusEnum.LOCKED);
                        if (!inProgressMissions.Any() && lockedMissions.Any())
                        {
                            var newlyAddedMissions = lockedMissions.Where(m => m.AssociatedLevel == Progress.CurrentPlayerLevel);
                            foreach (Mission mission in newlyAddedMissions)
                            {
                                mission.Status = MissionUnlockStatusEnum.IN_PROGRESS;
                            }
                        }

                        Progress.UnlockedOrcas.AddRange(DefaultUnlockedOrcas);
                        Progress.UnlockedOrcas = Progress.UnlockedOrcas.Distinct().ToList();
                    }
                    else
                    {
                        progressFile.Open(ProgressFileLocation, File.ModeFlags.Write);
                        Progress = CreateDefaultProgressObject();

                        string progressTextToSave = JsonConvert.SerializeObject(Progress);
                        progressFile.StoreString(progressTextToSave);
                    }

                    progressFile.Close();

                    return Progress;
                }
            }
        }

        private static List<OrcaEnum> DefaultUnlockedOrcas = new OrcaEnum[] { OrcaEnum.CoralOrca }.ToList();
        private static Progress CreateDefaultProgressObject()
        {
            return new Progress
            {
                CurrentPlayerLevel = 1,
                SingleSessionRecordStats = new StatDictionary(autoPopulateAllKeys: true),
                PointsSpent = 0,
                LifetimeCombinedStats = new StatDictionary(autoPopulateAllKeys: true),
                Missions = MissionDefinitions.CreateDefaultMissionList(),
                UnlockedOrcas = DefaultUnlockedOrcas
            };
        }

        public static void SaveSessionProgress()
        {
            lock (ProgressFileLock)
            {
                foreach (var statType in EnumExtensions.GetEnumValues<StatEnum>())
                {
                    var currentSessionStatValue = Progress.CurrentSessionStats[statType];

                    Progress.LifetimeCombinedStats.IncrementStat(statType, currentSessionStatValue);

                    bool noRecordValueSetForStat = !Progress.SingleSessionRecordStats.ContainsKey(statType);
                    if (noRecordValueSetForStat || Progress.SingleSessionRecordStats[statType] < currentSessionStatValue)
                    {
                        Progress.SingleSessionRecordStats[statType] = currentSessionStatValue;
                    }
                }

                const int LegendAchievementPointsThreshold = 5_000;
                if (Progress.CurrentSessionStats[StatEnum.Points] >= LegendAchievementPointsThreshold || Progress.SingleSessionRecordStats[StatEnum.Points] >= LegendAchievementPointsThreshold)
                {
                    SteamWrapper.UnlockAchievement(AchievementEnum.Legend);
                }

                var inProgressMissions = Progress.Missions.Where(m => m.Status == MissionUnlockStatusEnum.IN_PROGRESS).ToArray();
                foreach (var mission in inProgressMissions)
                {
                    mission.MissionProgress += mission.GetCurrentMissionProgress(Progress);
                    mission.RecalculateCompletionStatus(Progress);
                    if (mission.IsSingleSession)
                    {
                        mission.MissionProgress = 0;
                    }
                }

                if (Progress.RescuedOrca != null)
                {
                    NotificationHandler.QueueNotification($"[color=#1d3fad]{Progress.RescuedOrca.GetDescription()}[/color] has been rescued!");
                    CheckOrcaAchievements(Progress.RescuedOrca.Value);
                    Progress.UnlockedOrcas.Add(Progress.RescuedOrca.Value);
                    Progress.RescuedOrca = null;
                }

                bool currentMissionsFinished = inProgressMissions.All(m => m.IsUnlocked);
                if (currentMissionsFinished)
                {
                    if (inProgressMissions.Any())
                    {
                        Progress.CurrentPlayerLevel++;
                        NotifyUnlocksForNewLevel();
                    }

                    bool newMissionsAvailable = (Progress.CurrentPlayerLevel * ActiveMissionCount) <= Progress.Missions.Count;
                    if (newMissionsAvailable)
                    {
                        var newMissionIndices = Enumerable.Range((Progress.CurrentPlayerLevel - 1) * ActiveMissionCount, ActiveMissionCount);
                        var newMissions = newMissionIndices.Select(mId => Progress.Missions[mId]);
                        foreach (var mission in newMissions)
                        {
                            mission.Status = MissionUnlockStatusEnum.IN_PROGRESS;
                        }
                    }
                }

                Progress.IsCurrentSessionActive = false;

                SaveProgress();
            }
        }

        public static void NotifyUnlocksForNewLevel()
        {
            NotificationHandler.QueueNotification($"Level {Progress.CurrentPlayerLevel} reached!");

            var newlyUnlockedUnlockables = Progress.Unlockables.Where(u => u.LevelThreshold == Progress.CurrentPlayerLevel);
            foreach (var unlock in newlyUnlockedUnlockables)
            {
                if (unlock is Unlockable<BiomeEnum>)
                {
                    NotificationHandler.QueueNotification($"[color=#51ad00]{unlock.Description}[/color] biome is now available!");
                }

                if (unlock is Unlockable<PowerupEnum>)
                {
                    NotificationHandler.QueueNotification($"[color=#4949dd]{unlock.Description}[/color] powerup unlocked!");
                }

                if (unlock is Unlockable<OrcaEnum> orcaUnlock)
                {
                    NotificationHandler.QueueNotification($"[color=#1d3fad]{unlock.Description}[/color] has been unlocked!");
                    Progress.UnlockedOrcas.Add((OrcaEnum)unlock.EnumValue);
                    CheckOrcaAchievements(orcaUnlock.Value);
                }

                if (unlock is Unlockable<HatEnum>)
                {
                    NotificationHandler.QueueNotification($"[color=#ddd849]{unlock.Description}[/color] hat unlocked!");
                }
            }

            if (newlyUnlockedUnlockables.Any(u => u.Type == typeof(BiomeEnum) && Progress.AllBiomesUnlocked))
            {
                NotificationHandler.QueueNotification($"All biomes have been unlocked!");
            }
        }

        public static void CheckOrcaAchievements(OrcaEnum enumVal)
        {
            switch (enumVal)
            {
                case OrcaEnum.ArcticOrca:
                    SteamWrapper.UnlockAchievement(AchievementEnum.Heavyweight);
                    break;
                case OrcaEnum.CaveOrca:
                    SteamWrapper.UnlockAchievement(AchievementEnum.Kid);
                    break;
                case OrcaEnum.GrandmaOrca:
                    SteamWrapper.UnlockAchievement(AchievementEnum.Matriarch);
                    break;
                default:
                    break;
            }
        }

        public static void SaveProgress()
        {
            using (var progressFile = new File())
            {
                progressFile.Open(ProgressFileLocation, File.ModeFlags.Write);

                string progressTextToSave = JsonConvert.SerializeObject(Progress);
                progressFile.StoreString(progressTextToSave);

                progressFile.Close();

            }
        }
    }
}