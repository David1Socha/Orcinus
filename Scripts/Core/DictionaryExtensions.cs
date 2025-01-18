using System;
using System.Collections.Generic;

namespace Orcinus.Scripts.Core
{
    public static class DictionaryExtensions
    {
        public static TVal GetValueOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, TVal defaultValue)
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static void AddNewElementsFromDictionary<TKey, TValue>(this IDictionary<TKey, TValue> destinationDict, IDictionary<TKey, TValue> sourceDict, Action<TValue, TValue> mergeOldValueWithNew = null)
        {
            foreach (var kvp in sourceDict)
            {
                if (!(destinationDict.ContainsKey(kvp.Key)))
                {
                    destinationDict.Add(kvp.Key, kvp.Value);
                }
                else if (mergeOldValueWithNew != null)
                {
                    mergeOldValueWithNew(destinationDict[kvp.Key], kvp.Value);
                }
            }
        }
    }
}
