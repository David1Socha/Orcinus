using System.ComponentModel;

namespace Orcinus.Scripts.Models
{
    public enum OrcaEnum
    {
        Default = 99999,
        [Description("Sumi")]
        CoralOrca = 1,
        [Description("Finn")]
        ArcticOrca = 2,
        [Description("Nori")]
        CaveOrca = 3,
        [Description("Blanca")]
        GrandmaOrca = 4,
    }
}