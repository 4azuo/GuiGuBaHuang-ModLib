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

    /// <summary>Text of the key or of one of its variants (key_1, key_2...), chosen at random.</summary>
    public static string RandomLS(string key)
    {
        var keys = Variants(key);
        return GameTool.LS(keys[CommonTool.Random(0, keys.Count * 1000) % keys.Count]);
    }

    /// <summary>Spirit stone icon of the game (TextMeshPro sprite: drama texts only, not buttons nor message boxes).</summary>
    public const string STONE_ICON = "<sprite name=zm_lingshi>";

    /// <summary>"Spirit stones" in the mod texts, every language (the number before or after it).</summary>
    private const string STONE_WORD = @"(?:spirit\s+stones?|geistersteine?n?|piedras?\s+espirituales?|pierres?\s+spirituelles?|lapid\w*\s+spirital\w*|духовн\w*\s+кам\w*|หินวิญญาณ|linh\s+thạch|灵石|靈石|霊石|영석)";
    private const string NUMBER = @"[+\-]?\d(?:[\d,.   ]*\d)?";
    private static readonly System.Text.RegularExpressions.Regex NUMBER_STONES = new System.Text.RegularExpressions.Regex(
        "(" + NUMBER + @")\s*" + STONE_WORD, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    private static readonly System.Text.RegularExpressions.Regex STONES_NUMBER = new System.Text.RegularExpressions.Regex(
        STONE_WORD + @"\s*(" + NUMBER + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>
    /// A number of spirit stones in a drama text: "1,234 spirit stones" / "영석 1,234" -> the number and the icon.
    /// The word alone (no number next to it) stays. Set as ModLib's DramaHelper.TextFilter.
    /// </summary>
    public static string StoneIcons(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = NUMBER_STONES.Replace(text, m => m.Groups[1].Value + " " + STONE_ICON);
        return STONES_NUMBER.Replace(text, m => STONE_ICON + " " + m.Groups[1].Value);
    }

    /// <summary>Drama of a dialogue config with a random variant of its text (unit portraits left/right).</summary>
    public static void OpenDrama(int dramaId, string textKey, WorldUnitBase left, WorldUnitBase right)
    {
        DramaTool.OpenDrama(dramaId, new DramaData
        {
            dialogueText = { [dramaId] = StoneIcons(RandomLS(textKey)) },
            unitLeft = left,
            unitRight = right,
        });
    }
}
