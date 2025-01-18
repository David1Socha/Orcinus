using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Orcinus.Scripts.Models
{
    [JsonConverter(typeof(StatDictionaryConverter))]
    public class StatDictionary : Dictionary<StatEnum, int>
    {
        public StatDictionary() : this(autoPopulateAllKeys: true)
        {

        }

        public StatDictionary(bool autoPopulateAllKeys = true)
        {
            if (autoPopulateAllKeys)
            {
                var stats = EnumExtensions.GetEnumValues<StatEnum>();
                foreach (var stat in stats)
                {
                    this[stat] = 0;
                }
            }
        }

        public void IncrementStat(StatEnum statType, int delta = 1)
        {
            if (!ContainsKey(statType))
            {
                this[statType] = 0;
            }

            this[statType] += delta;

            // Don't allow negative stats
            this[statType] = Math.Max(0, this[statType]);
        }
    }
}
