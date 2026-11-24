using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>TextIconHelper: icon tokens of the localTexts (pure part; the TextMeshPro twins need the game).</summary>
    [TestClass]
    public class TextIconHelperTests
    {
        [TestMethod, Description("TextIconHelper.ToSprites: [icon:NAME] tokens -> <size=140%><sprite name=NAME></size> (any sprite name), other brackets and format items untouched")]
        public void ToSprites_ReplacesTokens()
        {
            Check.Eq("Pay {0:#,##0} <size=140%><sprite name=zm_lingshi></size>", TextIconHelper.ToSprites("Pay {0:#,##0} [icon:zm_lingshi]"), "one token, format item kept");
            Check.Eq("<size=140%><sprite name=zm_yaocai></size> 2, <size=140%><sprite name=lingli></size> 3", TextIconHelper.ToSprites("[icon:zm_yaocai] 2, [icon:lingli] 3"), "several tokens, name without zm_");
            Check.Eq("[note] [zm_lingshi] [icon:] [Icon:x]", TextIconHelper.ToSprites("[note] [zm_lingshi] [icon:] [Icon:x]"), "not tokens: unchanged");
            Check.Eq("plain", TextIconHelper.ToSprites("plain"), "no token");
            Check.Eq(null, TextIconHelper.ToSprites(null), "null");
            Check.Eq("1,234 <size=140%><sprite name=zm_lingshi></size>", string.Format(TextIconHelper.ToSprites("{0:#,##0} [icon:zm_lingshi]"), 1234), "string.Format after the conversion");
        }

        [TestMethod, Description("TextIconHelper.HasSprite: a text needs TextMeshPro when it has a sprite tag")]
        public void HasSprite_DetectsSpriteTags()
        {
            Check.Eq(true, TextIconHelper.HasSprite("5 <sprite name=zm_lingshi>"), "sprite");
            Check.Eq(false, TextIconHelper.HasSprite("5 [icon:zm_lingshi]"), "token not converted yet");
            Check.Eq(false, TextIconHelper.HasSprite("<b>bold</b>"), "other rich text");
            Check.Eq(false, TextIconHelper.HasSprite(null), "null");
        }
    }
}
