using Newtonsoft.Json;
using Orcinus.Scripts.Models.Missions;
using System.Collections.Generic;
using System.Linq;

namespace Orcinus.Scripts.Models
{
    public class Progress
    {
        [JsonIgnore]
        public StatDictionary CurrentSessionStats { get; set; }
        [JsonIgnore]
        public int PointsAvailable
        {
            get { return LifetimeCombinedStats[StatEnum.Points] - PointsSpent; }
        }

        [JsonIgnore]
        public int FishAvailable
        {
            get { return LifetimeCombinedStats[StatEnum.Fish] - FishSpent; }
        }

        [JsonIgnore]
        public bool IsCurrentSessionActive { get; set; } = true;

        [JsonIgnore]
        public BiomeEnum MaxUnlockedBiome
        {
            get
            {
                return UnlockedBiomes.Max();
            }
        }

        [JsonIgnore]
        public IEnumerable<BiomeEnum> UnlockedBiomes
        {
            get
            {
                return Unlockables.OfType<Unlockable<BiomeEnum>>().Where(u => u.LevelThreshold <= CurrentPlayerLevel).Select(u => u.Value);
            }
        }

        [JsonIgnore]
        public IEnumerable<PowerupEnum> UnlockedPowerups
        {
            get
            {
                return Unlockables.OfType<Unlockable<PowerupEnum>>().Where(u => u.LevelThreshold <= CurrentPlayerLevel).Select(u => u.Value);
            }
        }

        [JsonIgnore]
        public IEnumerable<HatEnum> UnlockedHats
        {
            get
            {
                return Unlockables.OfType<Unlockable<HatEnum>>().Where(u => u.LevelThreshold <= CurrentPlayerLevel).Select(u => u.Value);
            }
        }

        [JsonIgnore]
        public IEnumerable<Unlockable<OrcaEnum>> UnlockableOrcas
        {
            get
            {
                return Unlockables.OfType<Unlockable<OrcaEnum>>().Where(u => u.BiomeThreshold <= MaxUnlockedBiome);
            }
        }

        [JsonIgnore]
        public readonly IUnlockable[] Unlockables = new IUnlockable[]
        {
            new Unlockable<BiomeEnum>(1, BiomeEnum.Test),
            new Unlockable<BiomeEnum>(1, BiomeEnum.Coral),
            new Unlockable<BiomeEnum>(3, BiomeEnum.Arctic),
            new Unlockable<BiomeEnum>(6, BiomeEnum.Cave),

            new Unlockable<OrcaEnum>(BiomeEnum.Coral, OrcaEnum.CoralOrca),
            new Unlockable<OrcaEnum>(BiomeEnum.Arctic, OrcaEnum.ArcticOrca),
            new Unlockable<OrcaEnum>(BiomeEnum.Cave, OrcaEnum.CaveOrca),
            new Unlockable<OrcaEnum>(7, OrcaEnum.GrandmaOrca),

            new Unlockable<PowerupEnum>(1, PowerupEnum.HealthPack),
            new Unlockable<PowerupEnum>(1, PowerupEnum.Magnet),
            new Unlockable<PowerupEnum>(1, PowerupEnum.DoubleFish),
            new Unlockable<PowerupEnum>(2, PowerupEnum.Shield),
            new Unlockable<PowerupEnum>(4, PowerupEnum.SlowMotion),
            new Unlockable<PowerupEnum>(5, PowerupEnum.Ghost),
            new Unlockable<PowerupEnum>(7, PowerupEnum.TripleFish),

            new Unlockable<HatEnum>(1, HatEnum.None),
            new Unlockable<HatEnum>(1, HatEnum.Top),
            new Unlockable<HatEnum>(2, HatEnum.Chef),
            new Unlockable<HatEnum>(3, HatEnum.Cowhand),
            new Unlockable<HatEnum>(4, HatEnum.Magician),
            new Unlockable<HatEnum>(5, HatEnum.Bow),
            new Unlockable<HatEnum>(6, HatEnum.Party),
            new Unlockable<HatEnum>(7, HatEnum.Santa),
            new Unlockable<HatEnum>(8, HatEnum.Jester),
            new Unlockable<HatEnum>(8, HatEnum.Crown),
        };

        [JsonIgnore]
        public OrcaEnum? RescuedOrca { get; set; }

        [JsonIgnore]
        public int MissionLevel
        {
            get
            {
                return CurrentPlayerLevel > Missions.Count / 3 ? Missions.Count / 3 : CurrentPlayerLevel;
            }
        }
        public StatDictionary SingleSessionRecordStats { get; set; }
        public StatDictionary LifetimeCombinedStats { get; set; }

        // TODO should also track lifetime and session stats on a per-orca basis

        public List<Mission> Missions { get; set; }
        public int CurrentPlayerLevel { get; set; }
        [JsonIgnore]
        public bool AllBiomesUnlocked { get { return Unlockables.Count(u => u.Type == typeof(BiomeEnum)) == UnlockedBiomes.Count(); } }
        public int PointsSpent { get; set; }
        public int FishSpent { get; set; }
        public List<OrcaEnum> UnlockedOrcas { get; set; }

        [JsonIgnore]
        public OrcaEnum? UnlockableOrca
        {
            get
            {
                return UnlockableOrcas.FirstOrDefault(uo => !UnlockedOrcas.Contains((OrcaEnum)uo.EnumValue))?.Value;
            }
        }

    }
}
