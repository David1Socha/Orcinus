using System.ComponentModel;

namespace Orcinus.Scripts.Models
{
    public enum PowerupEnum
    {
        [Description("Health Pack")]
        HealthPack,
        [Description("Double Fish")]
        DoubleFish,
        [Description("Triple Fish")]
        TripleFish,
        [Description("Shield")]
        Shield,
        [Description("Magnet")]
        Magnet,
        [Description("Slow Motion")]
        SlowMotion,
        [Description("Ghost")]
        Ghost,
        Default = 99999,
    }
}
