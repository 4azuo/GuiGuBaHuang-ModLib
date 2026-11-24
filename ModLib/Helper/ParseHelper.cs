using ModLib.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for parsing and type conversion.
    /// Provides utilities for converting strings to various types and JSON parsing.
    /// </summary>
    [ActionCat("Parse")]
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
        /// Converts object to type O using JSON serialization.
        /// Creates a new variable instance.
        /// </summary>
        /// <typeparam name="O">Target type</typeparam>
        /// <param name="value">Source value</param>
        /// <returns>Converted value</returns>
        public static O ParseJson<O>(this object value)
        {
            return JsonConvert.DeserializeObject<O>(JsonConvert.SerializeObject(value, JSON_PARSER_CONF));
        }

        /// <summary>
        /// Converts object to specified type using JSON serialization.
        /// </summary>
        /// <param name="value">Source value</param>
        /// <param name="type">Target type</param>
        /// <returns>Converted value</returns>
        public static object ParseJson(this object value, Type type)
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, JSON_PARSER_CONF), type);
        }

        /// <summary>
        /// Converts IL2CPP object to type O using IL2CPP JSON.
        /// </summary>
        /// <typeparam name="O">Target type</typeparam>
        /// <param name="value">IL2CPP source</param>
        /// <returns>Converted value</returns>
        public static O Il2CppParseJson<O>(this Il2CppSystem.Object value)
        {
            return Il2CppNewtonsoft.Json.JsonConvert.DeserializeObject<O>(Il2CppNewtonsoft.Json.JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Converts IL2CPP object to specified IL2CPP type.
        /// </summary>
        /// <param name="value">IL2CPP source</param>
        /// <param name="type">IL2CPP target type</param>
        /// <returns>Converted value</returns>
        public static object Il2CppParseJson(this Il2CppSystem.Object value, Il2CppSystem.Type type)
        {
            return Il2CppNewtonsoft.Json.JsonConvert.DeserializeObject(Il2CppNewtonsoft.Json.JsonConvert.SerializeObject(value), type);
        }

        /// <summary>
        /// Converts value to specified type using reflection.
        /// </summary>
        /// <param name="value">Source value</param>
        /// <param name="toType">Target type</param>
        /// <returns>Converted value</returns>
        public static object ParseUnknown(this object value, Type toType)
        {
            return PARSER.MakeGenericMethod(toType).Invoke(null, new object[] { value, null });
        }

        /// <summary>
        /// Tries to convert value to specified type.
        /// </summary>
        /// <param name="value">Source value</param>
        /// <param name="toType">Target type</param>
        /// <param name="rs">Converted result</param>
        /// <returns>True if successful</returns>
        public static bool TryParseUnknown(this object value, Type toType, out object rs)
        {
            try
            {
                rs = value.ParseUnknown(toType);
                return true;
            }
            catch
            {
                rs = null;
                return false;
            }
        }

        /// <summary>
        /// Converts value to type O using TypeConverter.
        /// </summary>
        /// <typeparam name="O">Target type</typeparam>
        /// <param name="value">Source value</param>
        /// <param name="def">Default value on failure</param>
        /// <returns>Converted value</returns>
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
                //DebugHelper.WriteLine($"ParseHelper.Parse<{typeof(O).Name}>({value}, {default(O)})");
                //DebugHelper.WriteLine(ex);
                return def;
            }
        }

        /// <summary>
        /// Converts object to JSON string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJson(this object value, JsonSerializerSettings stts = null)
        {
            if (stts == null)
                stts = JSON_PARSER_CONF;
            return JsonConvert.SerializeObject(value, stts);
        }
    }
}