using System;
using System.Reflection;

public static class ObjectHelper
{
    public static readonly Newtonsoft.Json.JsonSerializerSettings CLONE_JSON_SETTINGS = new Newtonsoft.Json.JsonSerializerSettings
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
        PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
    };

    public static T Clone<T>(this T obj)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj, CLONE_JSON_SETTINGS), CLONE_JSON_SETTINGS);
    }

    public static void Map<T>(T src, T dest)
    {
        Map(src, dest, typeof(T));
    }

    public static void Map(object src, object dest, System.Type objType)
    {
        foreach (var p in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanWrite || !p.CanRead)
                continue;
            var srcValue = p.GetValue(src);
            if (srcValue != null)
                p.SetValue(dest, srcValue);
        }
    }

    public static void MapBySourceProp(object src, object dest)
    {
        foreach (var p1 in src.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var p2 = dest.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (p2 == null || !p2.CanWrite || !p1.CanRead)
                continue;
            var srcValue = p1.GetValue(src);
            if (srcValue != null)
                p2.SetValue(dest, srcValue);
        }
    }

    public static void MapByDestProp(object src, object dest)
    {
        foreach (var p1 in dest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var p2 = src.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (p2 == null || !p1.CanWrite || !p2.CanRead)
                continue;
            var srcValue = p2.GetValue(src);
            if (srcValue != null)
                p1.SetValue(dest, srcValue);
        }
    }

    public static object GetValue(this object obj, string fieldNm, bool ignoreError = false)
    {
        var prop = obj.GetType().GetProperty(fieldNm);
        if (prop == null)
        {
            if (ignoreError)
                return null;
            throw new NullReferenceException();
        }
        return prop.GetValue(obj);
    }

    public static void SetValue(this object obj, string fieldNm, object newValue, bool ignoreError = false)
    {
        var prop = obj.GetType().GetProperty(fieldNm);
        if (prop == null)
        {
            if (ignoreError)
                return;
            throw new NullReferenceException();
        }
        var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
        prop.SetValue(obj, ParseHelper.ParseUnknown(newValue, type));
    }

    public static bool IsDeclaredMethod(this object obj, string medName)
    {
        return obj.GetType().GetMethod(medName).DeclaringType == obj.GetType();
    }
}