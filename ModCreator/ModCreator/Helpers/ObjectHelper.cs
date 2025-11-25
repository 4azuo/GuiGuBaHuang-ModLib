using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for object operations
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Compare all public properties between two objects of the same type
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>True if all properties are equal, false otherwise</returns>
        public static bool ArePropertiesEqual<T>(T obj1, T obj2, params string[] ignoreProperties) where T : class
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            if (ReferenceEquals(obj1, obj2)) return true;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !ignoreProperties.Contains(p.Name));

            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreValuesEqual(value1, value2))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get a dictionary of property differences between two objects
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>Dictionary with property names as keys and tuple of (value1, value2) as values</returns>
        public static Dictionary<string, (object value1, object value2)> GetPropertyDifferences<T>(T obj1, T obj2, params string[] ignoreProperties) where T : class
        {
            var differences = new Dictionary<string, (object, object)>();

            if (obj1 == null || obj2 == null) return differences;
            if (ReferenceEquals(obj1, obj2)) return differences;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !ignoreProperties.Contains(p.Name));

            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreValuesEqual(value1, value2))
                {
                    differences[property.Name] = (value1, value2);
                }
            }

            return differences;
        }

        /// <summary>
        /// Copy all public properties from source to target object
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="source">Source object to copy from</param>
        /// <param name="target">Target object to copy to</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in copy</param>
        public static void CopyProperties<T>(T source, T target, params string[] ignoreProperties) where T : class
        {
            if (source == null || target == null) return;
            if (ReferenceEquals(source, target)) return;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !ignoreProperties.Contains(p.Name));

            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }

        /// <summary>
        /// Copy properties from source to target, only copying specified properties
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="source">Source object to copy from</param>
        /// <param name="target">Target object to copy to</param>
        /// <param name="propertiesToCopy">List of property names to copy</param>
        public static void CopyPropertiesSelective<T>(T source, T target, params string[] propertiesToCopy) where T : class
        {
            if (source == null || target == null) return;
            if (ReferenceEquals(source, target)) return;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && propertiesToCopy.Contains(p.Name));

            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }

        /// <summary>
        /// Deep clone an object using reflection (for simple objects without circular references)
        /// </summary>
        /// <typeparam name="T">Type of object to clone</typeparam>
        /// <param name="source">Source object to clone</param>
        /// <returns>Cloned object</returns>
        public static T DeepClone<T>(T source) where T : class, new()
        {
            if (source == null) return null;

            var clone = new T();
            CopyProperties(source, clone);
            return clone;
        }

        /// <summary>
        /// Compare two values for equality, handling nulls and collections
        /// </summary>
        public static bool AreValuesEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Handle collections
            if (value1 is System.Collections.IEnumerable enum1 && value2 is System.Collections.IEnumerable enum2)
            {
                var list1 = enum1.Cast<object>().ToList();
                var list2 = enum2.Cast<object>().ToList();

                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!AreValuesEqual(list1[i], list2[i]))
                        return false;
                }

                return true;
            }

            return Equals(value1, value2);
        }

        /// <summary>
        /// Check if any property has changed between two objects
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>True if at least one property is different, false otherwise</returns>
        public static bool HasPropertyChanges<T>(T obj1, T obj2, params string[] ignoreProperties) where T : class
        {
            return !ArePropertiesEqual(obj1, obj2, ignoreProperties);
        }

        /// <summary>
        /// Get list of property names that have different values between two objects
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>List of property names with different values</returns>
        public static List<string> GetChangedPropertyNames<T>(T obj1, T obj2, params string[] ignoreProperties) where T : class
        {
            var differences = GetPropertyDifferences(obj1, obj2, ignoreProperties);
            return differences.Keys.ToList();
        }
    }
}
