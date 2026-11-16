using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Object
{
    /// <summary>
    /// Base of "smart enums": static properties holding instances with values and extra data. Instances are registered in AllEnums.
    /// </summary>
    public abstract class EnumObject
    {
        #region Collector
        public static List<EnumObject> AllEnums { get; } = new List<EnumObject>();
        #endregion

        public string Value { get; private set; }
        public string Name
        {
            get
            {
                return this.GetType()
                    .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(x => x.GetValue(null) as EnumObject == this)?.Name;
            }
        }

        /// <summary>
        /// Creates and registers the instance (optionally with a value).
        /// </summary>
        public EnumObject()
        {
            Init();
        }

        /// <summary>
        /// Creates and registers the instance (optionally with a value).
        /// </summary>
        public EnumObject(string value)
        {
            Init(value);
        }

        /// <summary>
        /// Stores the value and registers the instance.
        /// </summary>
        private void Init(string value = null)
        {
            Value = value;
            AllEnums.Add(this);
        }

        /// <summary>
        /// All instances declared as static properties of T.
        /// </summary>
        public static T[] GetAllEnums<T>() where T : EnumObject
        {
            var t = typeof(T);
            var properties = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return properties.Where(x => t.IsAssignableFrom(x.PropertyType)).Select(x => (T)x.GetValue(null, null)).ToArray();
        }

        /// <summary>
        /// Instance of T with the given Value.
        /// </summary>
        public static T GetEnumByVal<T>(string val) where T : EnumObject
        {
            return GetAllEnums<T>().FirstOrDefault(x => x.Value == val);
        }

        /// <summary>
        /// Instance of T with the given property name.
        /// </summary>
        public static T GetEnumByName<T>(string nm) where T : EnumObject
        {
            return GetAllEnums<T>().FirstOrDefault(x => x.Name == nm);
        }

        /// <summary>
        /// Instance of T whose property equals the value.
        /// </summary>
        public static T GetEnumByProp<T>(string propName, object propValue) where T : EnumObject
        {
            var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return GetAllEnums<T>().FirstOrDefault(x => prop.GetValue(x, null) == propValue);
        }

        /// <summary>
        /// Instance of T by property name (throws when missing).
        /// </summary>
        public static T GetEnum<T>(string enumName) where T : EnumObject
        {
            var property = typeof(T).GetProperty(enumName, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return (T)property.GetValue(null, null);
        }

        /// <summary>
        /// TypeFullName.Name
        /// </summary>
        public override string ToString()
        {
            return $"{this.GetType().FullName}.{this.Name}";
        }
    }
}
