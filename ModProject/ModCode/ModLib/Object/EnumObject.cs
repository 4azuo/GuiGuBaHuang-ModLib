using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Object
{
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

        public EnumObject()
        {
            Init();
        }

        public EnumObject(string value)
        {
            Init(value);
        }

        private void Init(string value = null)
        {
            Value = value;
            AllEnums.Add(this);
        }

        public static T[] GetAllEnums<T>() where T : EnumObject
        {
            var t = typeof(T);
            var properties = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return properties.Where(x => t.IsAssignableFrom(x.PropertyType)).Select(x => (T)x.GetValue(null, null)).ToArray();
        }

        public static T GetEnumByVal<T>(string val) where T : EnumObject
        {
            return GetAllEnums<T>().FirstOrDefault(x => x.Value == val);
        }

        public static T GetEnumByProp<T>(string propName, object propValue) where T : EnumObject
        {
            var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return GetAllEnums<T>().FirstOrDefault(x => prop.GetValue(x, null) == propValue);
        }

        public static T GetEnum<T>(string enumName) where T : EnumObject
        {
            var property = typeof(T).GetProperty(enumName, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return (T)property.GetValue(null, null);
        }

        public override string ToString()
        {
            return $"{this.GetType().FullName}.{this.Name}";
        }
    }
}
