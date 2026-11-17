using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Taoist
{
    /// <summary>Taoist text helpers (no game state).</summary>
    [TestClass]
    public class TextTests
    {
        private const string I = ModTextHelper.STONE_ICON;

        [TestMethod, Description("ModTextHelper.StoneIcons: an amount of spirit stones in a drama text -> the number and the icon, every language; the word alone stays")]
        public void StoneIcons_ReplacesAmounts()
        {
            Check.Eq($"Everyone looks at you. (the bill: 268 {I})", ModTextHelper.StoneIcons("Everyone looks at you. (the bill: 268 spirit stones)"), "en, number before");
            Check.Eq($"(+1,200 exp, +3,456 {I})", ModTextHelper.StoneIcons("(+1,200 exp, +3,456 spirit stones)"), "en, signed, exp untouched");
            Check.Eq($"Pay 1 {I}", ModTextHelper.StoneIcons("Pay 1 Spirit Stone"), "en, singular, capitals");
            Check.Eq($"（账单：268 {I}）", ModTextHelper.StoneIcons("（账单：268灵石）"), "ch, no space");
            Check.Eq($"(계산서: {I} 268)", ModTextHelper.StoneIcons("(계산서: 영석 268)"), "kr, number after");
            Check.Eq($"（勘定：{I} 1,000）", ModTextHelper.StoneIcons("（勘定：霊石1,000）"), "ja, number after");
            Check.Eq($"(hóa đơn: 268 {I})", ModTextHelper.StoneIcons("(hóa đơn: 268 linh thạch)"), "vi");
            Check.Eq($"«1 000 {I}. Сегодня».", ModTextHelper.StoneIcons("«1 000 духовных камней. Сегодня»."), "ru, inflected, space separator");
            Check.Eq($"(die Rechnung: 268 {I})", ModTextHelper.StoneIcons("(die Rechnung: 268 Geistersteine)"), "de");
            Check.Eq($"(la cuenta: 268 {I})", ModTextHelper.StoneIcons("(la cuenta: 268 piedras espirituales)"), "es");
            Check.Eq($"(ratio: 268 {I})", ModTextHelper.StoneIcons("(ratio: 268 lapides spiritales)"), "la");
            Check.Eq("You don't have enough spirit stones.", ModTextHelper.StoneIcons("You don't have enough spirit stones."), "no number: unchanged");
            Check.Eq("영석이 부족하다.", ModTextHelper.StoneIcons("영석이 부족하다."), "kr, no number: unchanged");
            Check.Eq(null, ModTextHelper.StoneIcons(null), "null");
        }
    }
}
