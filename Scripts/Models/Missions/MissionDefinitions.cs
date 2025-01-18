
using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Orcinus.Scripts.Models.Missions
{
    public static class MissionDefinitions
    {
        private static List<Mission> _missionList = null;
        private const string MissionDefinitionsFileLocation = "res://Resources/mission_definitions.json";

        public static List<Mission> CreateDefaultMissionList()
        {
            if (_missionList != null)
            {
                return _missionList;
            }

            List<MissionDefinition> missionDefinitions = null;

            using (var missionFile = new File())
            {
                missionFile.Open(MissionDefinitionsFileLocation, File.ModeFlags.Read);
                string missionFileText = missionFile.GetAsText();
                missionDefinitions = JsonConvert.DeserializeObject<List<MissionDefinition>>(missionFileText);
            }
            var missionIds = Enumerable.Range(0, missionDefinitions.Count);

            var missions = missionIds.Select(mId =>
            {
                var missionDefinition = missionDefinitions[mId];
                return new Mission
                {
                    MissionId = mId,
                    Status = mId <= 2 ? MissionUnlockStatusEnum.IN_PROGRESS : MissionUnlockStatusEnum.LOCKED,
                    AssociatedStat = missionDefinition.AssociatedStat,
                    AssociatedOrca = missionDefinition.AssociatedOrca,
                    MissionDescription = missionDefinition.Description
                        .Replace("XX", $"[b]{missionDefinition.Threshold}[/b]")
                        .Replace("WW", $"[b]{missionDefinition.AssociatedOrca?.GetDescription()}[/b]"),
                    IsSingleSession = missionDefinition.IsSingleSession,
                    MissionThreshold = missionDefinition.Threshold
                };
            });
            _missionList = missions.ToList();

            return _missionList;
        }
    }
}
