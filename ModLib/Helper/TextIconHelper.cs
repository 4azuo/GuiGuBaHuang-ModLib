using ModLib.Attributes;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Icons of the game in texts. A localText writes an icon as a token "[icon:NAME]", NAME = a sprite of the game's
    /// TextMeshPro sprite asset (zm_lingshi spirit stone, zm_yaocai, zm_kuangshi, lingli, daodian...; square brackets:
    /// string.Format leaves them alone); at load the tokens of every localText become "&lt;sprite name=NAME&gt;".
    /// TextMeshPro texts (dramas, TextInfo) show it as is; a legacy
    /// uGUI Text gets a TextMeshPro twin (child "ModLibTMP", copied from UISampleHelper.TmpSample) that shows the text
    /// instead: Text.Set (CompHelper) and the message boxes / drama options go through SetText.
    /// </summary>
    [ActionCatIgn]
    public static class TextIconHelper
    {
        public const string TWIN_NAME = "ModLibTMP";
        public const string TOKEN_PREFIX = "[icon:";
        private static readonly Regex TOKEN = new Regex(@"\[icon:([A-Za-z0-9_]+)\]");
        /// <summary>The sprite tag of a token: a bit bigger than the text (the game wraps its own icons in a size tag too).</summary>
        private const string SPRITE = "<size=140%><sprite name=$1></size>";

        /// <summary>"[icon:NAME]" tokens -> TextMeshPro sprite tags.</summary>
        public static string ToSprites(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(TOKEN_PREFIX, StringComparison.Ordinal) < 0) return text;
            return TOKEN.Replace(text, SPRITE);
        }

        /// <summary>An icon of the game in a text (a sprite tag: shown by the TextMeshPro twin), e.g. Icon("huolinggen").</summary>
        public static string Icon(string name) => string.IsNullOrEmpty(name) ? string.Empty : ToSprites(TOKEN_PREFIX + name + "]");

        /// <summary>A text in a colour ("#7a1f1f", "red").</summary>
        public static string Paint(string color, string text) => string.IsNullOrEmpty(color) ? text : $"<color={color}>{text}</color>";

        /// <summary>
        /// The text icon of one of the 12 attributes of the game (spiritual roots fire..wood, weapon arts blade..finger: see
        /// HitHelper.ATTRIBUTES; "froze" = frost), <paramref name="fallback"/> (spirit power) for another.
        /// </summary>
        public static string AttributeIcon(string attribute, string fallback = "lingli")
        {
            switch (attribute?.ToLowerInvariant())
            {
                case "fire": return "huolinggen";
                case "frost": case "froze": return "shuilinggen";
                case "thunder": return "leilinggen";
                case "wind": return "fenglinggen";
                case "earth": return "tulinggen";
                case "wood": return "mulinggen";
                case "sword": return "jianfa";
                case "blade": return "daofa";
                case "spear": return "qiangfa";
                case "fist": return "quanfa";
                case "palm": return "zhangfa";
                case "finger": return "zhifa";
                default: return fallback;
            }
        }

        /// <summary>The text has a sprite tag (needs TextMeshPro).</summary>
        public static bool HasSprite(string text) => !string.IsNullOrEmpty(text) && text.IndexOf("<sprite", StringComparison.Ordinal) >= 0;

        /// <summary>Converts the icon tokens of every localText (all languages of the game). Returns the texts changed.</summary>
        public static int ConvertLocalTexts()
        {
            var sw = Stopwatch.StartNew();
            var n = 0;
            foreach (var kv in g.conf.localText.allText)
            {
                var item = kv.Value;
                if (item == null) continue;
                var changed = false;
                var en = item.en; if (Has(en)) { item.en = ToSprites(en); changed = true; }
                var ch = item.ch; if (Has(ch)) { item.ch = ToSprites(ch); changed = true; }
                var tc = item.tc; if (Has(tc)) { item.tc = ToSprites(tc); changed = true; }
                var kr = item.kr; if (Has(kr)) { item.kr = ToSprites(kr); changed = true; }
                if (changed) n++;
            }
            DebugHelper.WriteLine($"TextIconHelper: {n} localText(s) with icons ({sw.ElapsedMilliseconds} ms)");
            return n;
        }

        private static bool Has(string s) => s != null && s.IndexOf(TOKEN_PREFIX, StringComparison.Ordinal) >= 0;

        /// <summary>The TextMeshPro twin of a Text (null when none).</summary>
        public static TextMeshProUGUI GetTwin(Text t)
        {
            if (t == null) return null;
            var tr = t.transform.Find(TWIN_NAME);
            return tr == null ? null : tr.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>The text shown by a Text: its TextMeshPro twin's when the twin shows it (a text with an icon), else its own.</summary>
        public static string GetShownText(Text t)
        {
            if (t == null) return string.Empty;
            var tw = GetTwin(t);
            return tw != null && tw.gameObject.activeSelf && string.IsNullOrEmpty(t.text) ? tw.text ?? string.Empty : t.text ?? string.Empty;
        }

        /// <summary>
        /// Sets the text of a Text: with a sprite, shown by its TextMeshPro twin (created once, same rect, font size,
        /// color, alignment and wrapping) and the Text itself left empty; without, the Text as usual (twin hidden).
        /// </summary>
        public static void SetText(Text t, string text)
        {
            if (t == null) return;
            if (HasSprite(text) && UISampleHelper.TmpSample != null)
            {
                var tw = GetTwin(t) ?? CreateTwin(t);
                if (tw != null)
                {
                    Sync(t, tw);
                    tw.text = text;
                    tw.gameObject.SetActive(true);
                    t.text = string.Empty;
                    return;
                }
            }
            var twin = GetTwin(t);
            if (twin != null && twin.gameObject.activeSelf) twin.gameObject.SetActive(false);
            t.text = text;
        }

        /// <summary>The twin follows the Text's look (after a color / size / alignment change).</summary>
        public static void Sync(Text t)
        {
            var tw = GetTwin(t);
            if (tw != null && tw.gameObject.activeSelf) Sync(t, tw);
        }

        /// <summary>
        /// The texts of a game UI set by the game (drama options...): a text with a sprite moves to its twin; a twin
        /// whose Text got a new plain text is hidden.
        /// </summary>
        public static void Refresh(Component root)
        {
            if (root == null) return;
            foreach (var t in root.GetComponentsInChildren<Text>(true))
            {
                var s = t.text;
                if (HasSprite(s)) SetText(t, s);
                else if (!string.IsNullOrEmpty(s))
                {
                    var tw = GetTwin(t);
                    if (tw != null && tw.gameObject.activeSelf) tw.gameObject.SetActive(false);
                }
            }
        }

        private static TextMeshProUGUI CreateTwin(Text t)
        {
            var tw = UnityEngine.Object.Instantiate(UISampleHelper.TmpSample, t.transform, false);
            tw.gameObject.name = TWIN_NAME;
            // the sample (tooltip text of UITextInfo) fits its size to its lines: the twin would shrink around the middle of
            // the Text and a long block would not start at its top; the twin keeps the rect of the Text instead
            foreach (var f in tw.GetComponents<ContentSizeFitter>()) f.enabled = false;
            var rt = tw.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = t.rectTransform.pivot;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            tw.raycastTarget = false;
            tw.richText = true;
            tw.overflowMode = TextOverflowModes.Overflow;
            return tw;
        }

        private static void Sync(Text t, TextMeshProUGUI tw)
        {
            tw.fontSize = t.fontSize;
            tw.color = t.color;
            tw.alignment = Alignment(t.alignment);
            tw.enableWordWrapping = t.horizontalOverflow == HorizontalWrapMode.Wrap;
            tw.fontStyle = t.fontStyle == FontStyle.Bold ? FontStyles.Bold
                : t.fontStyle == FontStyle.Italic ? FontStyles.Italic
                : t.fontStyle == FontStyle.BoldAndItalic ? FontStyles.Bold | FontStyles.Italic
                : FontStyles.Normal;
        }

        /// <summary>uGUI TextAnchor -> TextMeshPro alignment.</summary>
        public static TextAlignmentOptions Alignment(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
