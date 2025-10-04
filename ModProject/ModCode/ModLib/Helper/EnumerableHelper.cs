using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableHelper
{
    public static void RemoveKeysStartWith(this IDictionary<string, object> dict, string prefix)
    {
        if (dict == null || string.IsNullOrEmpty(prefix))
            return;

        var keysToRemove = dict.Keys
                               .Where(k => !string.IsNullOrEmpty(k) && k.StartsWith(prefix))
                               .ToList();

        foreach (var key in keysToRemove)
        {
            dict.Remove(key);
        }
    }

    public static void RemoveKeysStartWith<TValue>(this IDictionary<string, TValue> dict, string prefix)
    {
        if (dict == null || string.IsNullOrEmpty(prefix))
            return;

        var keysToRemove = dict.Keys
                               .Where(k => !string.IsNullOrEmpty(k) && k.StartsWith(prefix))
                               .ToList();

        foreach (var key in keysToRemove)
        {
            dict.Remove(key);
        }
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static IEnumerable<TSource> DistinctBySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        if (source == null) yield break;

        HashSet<TKey> seenKeys = new HashSet<TKey>();

        foreach (var element in source)
        {
            if (element == null) continue;

            TKey key;
            try
            {
                key = keySelector(element);
            }
            catch (NullReferenceException)
            {
                // Nếu keySelector bị null reference → bỏ qua
                continue;
            }

            if (key == null) continue;

            if (seenKeys.Add(key))
            {
                yield return element;
            }
        }
    }

    public static void AddRange<T>(this Il2CppSystem.Collections.Generic.List<T> lst, Il2CppSystem.Collections.Generic.List<T> addedlst)
    {
        foreach (var item in addedlst)
        {
            lst.Add(item);
        }
    }

    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this IEnumerable<T> lst)
    {
        var rs = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in lst)
        {
            rs.Add(item);
        }
        return rs;
    }

    public static Il2CppSystem.Collections.Generic.Dictionary<K, V> ToIl2CppDictionary<X, K, V>(this IEnumerable<X> lst, Func<X, K> predicateK, Func<X, V> predicateV)
    {
        var rs = new Il2CppSystem.Collections.Generic.Dictionary<K, V>();
        foreach (var item in lst)
        {
            rs.Add(predicateK(item), predicateV(item));
        }
        return rs;
    }

    public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> lst)
    {
        return lst.ToArray().ToList();
    }

    public static IList<V> ToList<K, V>(this Il2CppSystem.Collections.Generic.Dictionary<K, V>.ValueCollection collection)
    {
        var rs = new List<V>();
        foreach (var item in collection)
        {
            rs.Add(item);
        }
        return rs;
    }

    public static IList<K> ToList<K, V>(this Il2CppSystem.Collections.Generic.Dictionary<K, V>.KeyCollection collection)
    {
        var rs = new List<K>();
        foreach (var item in collection)
        {
            rs.Add(item);
        }
        return rs;
    }

    public static T Random<T>(this IList<T> lst)
    {
        if (lst == null || lst.Count == 0)
        {
            return default(T);
        }
        var r = CommonTool.Random(0, lst.Count - 1);
        return lst[r];
    }

    public static int IndexOf<T>(this IList<T> lst, Predicate<T> predicate)
    {
        for (var i = 0; i < lst.Count; i++)
        {
            if (predicate(lst[i]))
                return i;
        }
        return -1;
    }
}