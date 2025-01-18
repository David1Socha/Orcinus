using System;

namespace Orcinus.Scripts.Models
{
    public interface IUnlockable
    {
        int? LevelThreshold { get; set; }
        Type Type { get; }
        Enum EnumValue { get; set; }
        BiomeEnum? BiomeThreshold { get; set; }
        string Description { get; }
    }

    public class Unlockable<TType> : IUnlockable where TType : struct, IComparable
    {
        public Unlockable(int levelThreshold, TType unlockableItem)
        {
            LevelThreshold = levelThreshold;
            EnumValue = unlockableItem as Enum;
            Value = unlockableItem;
        }

        public Unlockable(BiomeEnum biomeThreshold, TType unlockableItem)
        {
            BiomeThreshold = biomeThreshold;
            EnumValue = unlockableItem as Enum;
            Value = unlockableItem;
        }

        public Type Type { get { return typeof(TType); } }
        public string Description { get { return EnumValue.GetDescription(); } }

        public int? LevelThreshold { get; set; }
        public Enum EnumValue { get; set; }
        public TType Value { get; set; }
        public BiomeEnum? BiomeThreshold { get; set; }
    }
}
