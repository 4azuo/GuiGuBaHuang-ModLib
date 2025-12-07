using ModLib.Attributes;
using System;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for object manipulation and reflection operations.
    /// Provides utilities for cloning, property setting, type casting, and object mapping.
    /// </summary>
    [ActionCat("Object")]
    public static class ObjectHelper
    {
        public static int goIndex = 0;

        /// <summary>
        /// JSON settings for deep cloning objects.
        /// </summary>
        public static readonly Newtonsoft.Json.JsonSerializerSettings CLONE_JSON_SETTINGS = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
            PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
        };

        /// <summary>
        /// Creates a deep clone of an object using JSON serialization.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="obj">The object to clone</param>
        /// <returns>A deep copy of the object</returns>
        public static T Clone<T>(this T obj)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj, CLONE_JSON_SETTINGS), CLONE_JSON_SETTINGS);
        }

        /// <summary>
        /// Maps (copies) properties from source to destination object.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="src">Source object</param>
        /// <param name="dest">Destination object</param>
        public static void Map<T>(T src, T dest)
        {
            Map(src, dest, typeof(T));
        }

        /// <summary>
        /// Maps properties from source to destination using specified type.
        /// Only copies non-null values.
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dest">Destination object</param>
        /// <param name="objType">Type to use for property discovery</param>
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

        /// <summary>
        /// Maps properties by matching source property names.
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dest">Destination object</param>
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

        /// <summary>
        /// Maps properties by matching destination property names.
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dest">Destination object</param>
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

        /// <summary>
        /// Gets a field by name from an object.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Field name</param>
        /// <returns>FieldInfo or null</returns>
        public static FieldInfo GetField(this object obj, string fieldNm)
        {
            return obj.GetType().GetField(fieldNm);
        }

        /// <summary>
        /// Gets a property by name from an object.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Property name</param>
        /// <returns>PropertyInfo or null</returns>
        public static PropertyInfo GetProperty(this object obj, string fieldNm)
        {
            return obj.GetType().GetProperty(fieldNm);
        }

        /// <summary>
        /// Gets a property value without null checking (unsafe).
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Property name</param>
        /// <returns>Property value</returns>
        public static object GetValueUnsafe(this object obj, string fieldNm)
        {
            return obj.GetType().GetProperty(fieldNm).GetValue(obj);
        }

        /// <summary>
        /// Gets a property value with null checking.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Property name</param>
        /// <param name="ignorePropertyNotFoundError">If true, returns null instead of throwing</param>
        /// <returns>Property value or null</returns>
        public static object GetValue(this object obj, string fieldNm, bool ignorePropertyNotFoundError = false)
        {
            var prop = obj.GetType().GetProperty(fieldNm);
            if (prop == null)
            {
                if (ignorePropertyNotFoundError)
                    return null;
                throw new NullReferenceException();
            }
            return prop.GetValue(obj);
        }

        /// <summary>
        /// Sets a property value without null checking (unsafe).
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Property name</param>
        /// <param name="newValue">New value</param>
        public static void SetValueUnsafe(this object obj, string fieldNm, object newValue)
        {
            obj.GetType().GetProperty(fieldNm).SetValue(obj, newValue);
        }

        /// <summary>
        /// Sets a property value with type parsing and null checking.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="fieldNm">Property name</param>
        /// <param name="newValue">New value (will be parsed to correct type)</param>
        /// <param name="ignorePropertyNotFoundError">If true, returns instead of throwing</param>
        /// <param name="customParser">Custom parser for unsupported types</param>
        public static void SetValue(this object obj, string fieldNm, object newValue, bool ignorePropertyNotFoundError = false, Func<Type, object> customParser = null)
        {
            var prop = obj.GetType().GetProperty(fieldNm);
            if (prop == null)
            {
                if (ignorePropertyNotFoundError)
                    return;
                throw new NullReferenceException();
            }
            var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
            if (ParseHelper.TryParseUnknown(newValue, type, out object parsedValue))
            {
                prop.SetValue(obj, parsedValue);
            }
            else
            {
                if (customParser == null)
                {
                    //ignore
                }
                else
                {
                    prop.SetValue(obj, customParser(type));
                }
            }
        }

        /// <summary>
        /// Checks if a method is declared in the object's type (not inherited).
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="medName">Method name</param>
        /// <returns>True if declared in this type</returns>
        public static bool IsDeclaredMethod(this object obj, string medName)
        {
            return obj?.GetType()?.GetMethod(medName)?.DeclaringType == obj.GetType();
        }

        /// <summary>
        /// Gets the compiler-generated backing field name for a property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Backing field name</returns>
        public static string GetBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        /// <summary>
        /// Gets the backing field for an auto-property.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>FieldInfo for backing field</returns>
        public static FieldInfo GetBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Sets the backing field value for an auto-property directly.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">New value</param>
        public static void SetBackingField(object obj, string propertyName, object value)
        {
            GetBackingField(obj, propertyName).SetValue(obj, value);
        }
    }
}