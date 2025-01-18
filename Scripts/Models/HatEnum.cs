using System.ComponentModel;

namespace Orcinus.Scripts.Models
{
    public enum HatEnum
    {
        [Description("None")]
        None = 0,
        [Description("Bow")]
        Bow = 1,
        [Description("Chef")]
        Chef = 2,
        [Description("Cowhand")]
        Cowhand = 3,
        [Description("Crown")]
        Crown = 4,
        [Description("Jester")]
        Jester = 5,
        [Description("Magician")]
        Magician = 6,
        [Description("Party")]
        Party = 7,
        [Description("Santa")]
        Santa = 8,
        [Description("Top")]
        Top = 9,
        Default = 9999,
    }
}
