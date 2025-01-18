using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Orcinus.Scripts.Models
{
    public class StatDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StatDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var statDict = new StatDictionary();
            var jObj = JObject.Load(reader);

            foreach (var o in jObj)
            {
                StatEnum stat = (StatEnum)int.Parse(o.Key);
                int value = o.Value.ToObject<int>();
                statDict[stat] = value;
            }

            return statDict;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var statDict = value as StatDictionary;

            writer.WriteStartObject();

            foreach (var kvp in statDict)
            {
                int statKeyInt = (int)kvp.Key;
                writer.WritePropertyName(statKeyInt.ToString());
                serializer.Serialize(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }
    }
}
