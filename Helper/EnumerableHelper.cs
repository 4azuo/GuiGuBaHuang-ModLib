using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableHelper
{
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