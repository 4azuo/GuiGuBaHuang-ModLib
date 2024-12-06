using System.Collections.Generic;

public static class EnumerableHelper
{
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this IEnumerable<T> lst)
    {
        var rs = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in lst)
        {
            rs.Add(item);
        }
        return rs;
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
}