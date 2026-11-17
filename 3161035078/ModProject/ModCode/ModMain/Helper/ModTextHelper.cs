using System.Collections.Generic;

/// <summary>
/// Taoist text shortcuts: random variants of a localText key.
/// </summary>
public static class ModTextHelper
{
    public const int MAX_VARIANTS = 9;

    /// <summary>True when the localText key exists (GameTool.LS returns the key itself otherwise).</summary>
    public static bool HasText(string key)
    {
        var s = GameTool.LS(key);
        return !string.IsNullOrEmpty(s) && s != key;
    }

    /// <summary>The key and its variants that exist: key, key_1, key_2...</summary>
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

    /// <summary>The text of a localText key filled with its format items (no hardcoded text in the code).</summary>
    public static string LSF(string key, params object[] args) => string.Format(GameTool.LS(key), args);

    /// <summary>Text of the key or of one of its variants (key_1, key_2...), chosen at random.</summary>
    public static string RandomLS(string key)
    {
        var keys = Variants(key);
        return GameTool.LS(keys[CommonTool.Random(0, keys.Count * 1000) % keys.Count]);
    }

    /// <summary>Drama of a dialogue config with a random variant of its text (unit portraits left/right).</summary>
    public static void OpenDrama(int dramaId, string textKey, WorldUnitBase left, WorldUnitBase right)
    {
        DramaTool.OpenDrama(dramaId, new DramaData
        {
            dialogueText = { [dramaId] = RandomLS(textKey) },
            unitLeft = left,
            unitRight = right,
        });
    }
}
