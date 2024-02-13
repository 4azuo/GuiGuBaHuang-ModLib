using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;

public static class ParseHelper
{
    public static readonly JsonSerializerSettings JSON_PARSER_CONF = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        NullValueHandling = NullValueHandling.Ignore,
    };

    #region Const
    public static readonly MethodInfo PARSER = typeof(ParseHelper).GetMethod("Parse", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    #endregion

    /// <summary>
    /// Oに変換する
    /// ※別な変数を作成する
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static O ParseJson<O>(this object value)
    {
        return JsonConvert.DeserializeObject<O>(JsonConvert.SerializeObject(value, JSON_PARSER_CONF));
    }
    public static object ParseJson(this object value, Type type)
    {
        return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, JSON_PARSER_CONF), type);
    }

    /// <summary>
    /// toTypeに変換する
    /// </summary>
    /// <param name="value"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static object ParseUnknown(this object value, Type toType)
    {
        return PARSER.MakeGenericMethod(toType).Invoke(null, new object[] { value, null });
    }

    /// <summary>
    /// Oに変換する
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <param name="value"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public static O Parse<O>(this object value, O def = default)
    {
        var t = typeof(O);
        var converter = TypeDescriptor.GetConverter(t);
        try
        {
            if (converter.CanConvertTo(t))
                return (O)converter.ConvertTo(value, t);
            if (converter.CanConvertFrom(t))
                return (O)converter.ConvertFrom(value);
            return (O)Convert.ChangeType(value, t);
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine($"ParseHelper.Parse<{typeof(O).Name}>({value}, {default(O)})");
            DebugHelper.WriteLine(ex);
            return def;
        }
    }
}