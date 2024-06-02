using System.Collections.Generic;

namespace Mp3Player.Helpers.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrChangeValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        where TKey : notnull
    {
        if (dic.TryGetValue(key, out TValue? outValue))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }
}