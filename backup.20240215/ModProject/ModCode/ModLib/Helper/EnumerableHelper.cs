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
}