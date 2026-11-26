using ModLib.Attributes;
using System.Collections.Generic;

namespace ModLib.Helper
{
    /// <summary>
    /// Shortcuts of the localTexts (GameTool.LS): a text filled with its format items, whether a key exists, random
    /// variants of a key (key, key_1, key_2...: a mod writes several lines for one thing and shows one at random).
    /// </summary>
    [ActionCatIgn]
    public static class LocalTextHelper
    {
        /// <summary>Most variants of a key (key_1 .. key_9).</summary>
        public const int MAX_VARIANTS = 9;

        /// <summary>True when the localText key exists (GameTool.LS returns the key itself otherwise).</summary>
        public static bool HasText(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var s = GameTool.LS(key);
            return !string.IsNullOrEmpty(s) && s != key;
        }

        /// <summary>The key and its variants that exist: key, key_1, key_2... (stops at the first missing one).</summary>
        public static List<string> Variants(string key)
        {
            var rs = new List<string> { key };
            for (var i = 1; i <= MAX_VARIANTS; i++)
            {
                var k = $"{key}_{i}";
                if (!HasText(k))
                    break;
                rs.Add(k);
            }
            return rs;
        }

        /// <summary>The text of a localText key filled with its format items ({0}, {1}...: no hardcoded text in the code).</summary>
        public static string LSF(string key, params object[] args) => string.Format(GameTool.LS(key), args);

        /// <summary>Text of the key or of one of its variants (key_1, key_2...), chosen at random.</summary>
        public static string RandomLS(string key)
        {
            var keys = Variants(key);
            return GameTool.LS(keys[CommonTool.Random(0, keys.Count * 1000) % keys.Count]);
        }
    }
}
