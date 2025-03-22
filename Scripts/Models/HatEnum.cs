using System.ComponentModel;

namespace Orcinus.Scripts.Models
{
    public enum HatEnum
    {
        [Description("None")]
        None = 0,
        [Description("Kelp")]
        Kelp = 1,
        [Description("Seaweed")]
        Seaweed = 2,
        [Description("Conch")]
        Conch = 3,
        [Description("Clam")]
        Clam = 4,
        [Description("Starfish")]
        Starfish = 5,
        [Description("Jellyfish")]
        Jellyfish = 6,
        [Description("Seashells")]
        Seashells = 7,
        [Description("Coral")]
        Coral = 8,
        [Description("Salmon")]
        Salmon = 9,
        [Description("Pebbles")]
        Pebbles = 10,
        Default = 9999,
    }
}
