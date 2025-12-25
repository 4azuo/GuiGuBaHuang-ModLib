using ModLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with collections and enumerables.
    /// Provides extension methods for dictionaries, lists, and LINQ operations.
    /// </summary>
    [ActionCat("Enumerable")]
    public static class EnumerableHelper
    {
        /// <summary>
        /// Copies the contents of a <see cref="NativeArray{Byte}"/> to a managed byte array.
        /// </summary>
        /// <remarks>This method creates a new managed array and copies all elements from the specified
        /// native array. The returned array is independent of the original native array; subsequent changes to one will
        /// not affect the other.</remarks>
        /// <param name="native">The native array whose contents will be copied to a managed array.</param>
        /// <returns>A managed byte array containing the copied elements from the native array. The length of the returned array
        /// matches the length of <paramref name="native"/>.</returns>
        public static unsafe byte[] NativeToManaged(NativeArray<byte> native)
        {
            int len = native.Length;
            byte[] managed = new byte[len];

            void* src = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(native);

            fixed (byte* dst = managed)
            {
                UnsafeUtility.MemCpy(dst, src, len);
            }

            return managed;
        }

        /// <summary>
        /// Removes all keys starting with the specified prefix from dictionary.
        /// </summary>
        /// <param name="dict">Dictionary</param>
        /// <param name="prefix">Key prefix</param>
        public static void RemoveKeysStartWith(this IDictionary<string, object> dict, string prefix)
        {
            if (dict == null || string.IsNullOrEmpty(prefix))
                return;

            var keysToRemove = dict.Keys
                                   .Where(k => !string.IsNullOrEmpty(k) && k.StartsWith(prefix))
                                   .ToList();

            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// Removes all keys starting with the specified prefix from typed dictionary.
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="prefix">Key prefix</param>
        public static void RemoveKeysStartWith<TValue>(this IDictionary<string, TValue> dict, string prefix)
        {
            if (dict == null || string.IsNullOrEmpty(prefix))
                return;

            var keysToRemove = dict.Keys
                                   .Where(k => !string.IsNullOrEmpty(k) && k.StartsWith(prefix))
                                   .ToList();

            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// Returns distinct elements based on a key selector.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Function to extract key</param>
        /// <returns>Distinct elements</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Returns distinct elements based on a key selector with null safety.
        /// Skips null elements and handles null reference exceptions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Function to extract key</param>
        /// <returns>Distinct elements</returns>
        public static IEnumerable<TSource> DistinctBySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) yield break;

            HashSet<TKey> seenKeys = new HashSet<TKey>();

            foreach (var element in source)
            {
                if (element == null) continue;

                TKey key;
                try
                {
                    key = keySelector(element);
                }
                catch (NullReferenceException)
                {
                    // Nếu keySelector bị null reference → bỏ qua
                    continue;
                }

                if (key == null) continue;

                if (seenKeys.Add(key))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Adds range of items to IL2CPP list.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="lst">Target list</param>
        /// <param name="addedlst">Items to add</param>
        public static void AddRange<T>(this Il2CppSystem.Collections.Generic.List<T> lst, Il2CppSystem.Collections.Generic.List<T> addedlst)
        {
            foreach (var item in addedlst)
            {
                lst.Add(item);
            }
        }

        /// <summary>
        /// Converts IEnumerable to IL2CPP list.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="lst">Source enumerable</param>
        /// <returns>IL2CPP list</returns>
        public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this IEnumerable<T> lst)
        {
            var rs = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in lst)
            {
                rs.Add(item);
            }
            return rs;
        }

        /// <summary>
        /// Converts IEnumerable to IL2CPP dictionary using key and value selectors.
        /// </summary>
        /// <typeparam name="X">Source type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="V">Value type</typeparam>
        /// <param name="lst">Source enumerable</param>
        /// <param name="predicateK">Key selector</param>
        /// <param name="predicateV">Value selector</param>
        /// <returns>IL2CPP dictionary</returns>
        public static Il2CppSystem.Collections.Generic.Dictionary<K, V> ToIl2CppDictionary<X, K, V>(this IEnumerable<X> lst, Func<X, K> predicateK, Func<X, V> predicateV)
        {
            var rs = new Il2CppSystem.Collections.Generic.Dictionary<K, V>();
            foreach (var item in lst)
            {
                rs.Add(predicateK(item), predicateV(item));
            }
            return rs;
        }

        /// <summary>
        /// Converts IL2CPP list to standard List.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="lst">IL2CPP list</param>
        /// <returns>Standard List</returns>
        public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> lst)
        {
            return lst.ToArray().ToList();
        }

        /// <summary>
        /// Converts IL2CPP dictionary value collection to standard List.
        /// </summary>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="V">Value type</typeparam>
        /// <param name="collection">Value collection</param>
        /// <returns>Standard List</returns>
        public static IList<V> ToList<K, V>(this Il2CppSystem.Collections.Generic.Dictionary<K, V>.ValueCollection collection)
        {
            var rs = new List<V>();
            foreach (var item in collection)
            {
                rs.Add(item);
            }
            return rs;
        }

        /// <summary>
        /// Converts IL2CPP dictionary key collection to standard List.
        /// </summary>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="V">Value type</typeparam>
        /// <param name="collection">Key collection</param>
        /// <returns>Standard List</returns>
        public static IList<K> ToList<K, V>(this Il2CppSystem.Collections.Generic.Dictionary<K, V>.KeyCollection collection)
        {
            var rs = new List<K>();
            foreach (var item in collection)
            {
                rs.Add(item);
            }
            return rs;
        }

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="lst">Source list</param>
        /// <returns>Random element, or default(T) if list is null or empty</returns>
        public static T Random<T>(this IList<T> lst)
        {
            if (lst == null || lst.Count == 0)
            {
                return default(T);
            }
            var r = CommonTool.Random(0, lst.Count - 1);
            return lst[r];
        }

        /// <summary>
        /// Finds the index of the first element matching the predicate.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="lst">Source list</param>
        /// <param name="predicate">Matching condition</param>
        /// <returns>Index of first match, or -1 if not found</returns>
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
}