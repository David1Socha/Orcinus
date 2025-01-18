namespace Orcinus.Scripts.Models.Missions
{
    public class MissionDefinition
    {
        public string Description { get; set; }
        public StatEnum AssociatedStat { get; set; }
        public int Threshold { get; set; }
        public bool IsSingleSession { get; set; }
        public OrcaEnum? AssociatedOrca { get; set; }

    }
}
