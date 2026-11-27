using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>HttpHelper: Workshop description of a page (no download).</summary>
    [TestClass]
    public class HttpHelperTests
    {
        private const string PAGE =
            "<div class=\"workshopItemDescriptionTitle\">Description</div>\n" +
            "<div class=\"workshopItemDescription\" id=\"highlightContent\"><div class=\"bb_h1\">Taoist — v5.5.40</div><br>A <b>hardcore rework</b> of the game.<br>Configs in <b>S&amp;M Configs</b>.<br><br>" +
            "<div class=\"bb_h2\">⚠ Requirements</div><ul class=\"bb_ul\"><li><a class=\"bb_link\" href=\"https://x\" target=\"_blank\" rel=\"\" >Fouru's ModLib</a> v2.2.27 or newer<br></li>" +
            "<li><b>Move ModLib to the TOP</b>, then restart.</li></ul><br><div class=\"bb_h2\">New game</div><ul class=\"bb_ul\"><li>Option A<br></li><li>Option B</li></ul></div>\n" +
            "<div class=\"after\">not the description</div>";

        [TestMethod, Description("HttpHelper.ExtractWorkshopDescription: the whole description block, nested divs (headings) included")]
        public void ExtractWorkshopDescription_KeepsNestedDivs()
        {
            var html = HttpHelper.ExtractWorkshopDescription(PAGE);
            Check.Eq(true, html.StartsWith("<div class=\"bb_h1\">Taoist"), "starts at the first heading");
            Check.Eq(true, html.Contains("Option B"), "goes past the inner </div> (headings)");
            Check.Eq(false, html.Contains("not the description"), "stops at the closing div of the block");
            Check.Eq(string.Empty, HttpHelper.ExtractWorkshopDescription("<html>no description</html>"), "no block");
            Check.Eq(string.Empty, HttpHelper.ExtractWorkshopDescription(null), "null page");
        }

        [TestMethod, Description("HttpHelper.WorkshopRichText: Steam's HTML -> TextMeshPro rich text (headings, bullets, line breaks, entities, no html tags)")]
        public void WorkshopRichText_ConvertsHtml()
        {
            var text = HttpHelper.WorkshopRichText(HttpHelper.ExtractWorkshopDescription(PAGE));
            Check.Info(text);
            Check.Eq(true, text.StartsWith("<size=140%><b>Taoist — v5.5.40</b></size>\n"), "h1: big bold line");
            Check.Eq(true, text.Contains("<size=120%><b>Requirements</b></size>\n"), "h2: bold line, symbol missing from the font removed");
            Check.Eq(true, text.Contains("  • Fouru's ModLib v2.2.27 or newer\n  • <b>Move ModLib to the TOP</b>, then restart."), "list items, link text kept, one line each");
            Check.Eq(true, text.Contains("S&M Configs"), "entities decoded");
            Check.Eq(false, text.Contains("<div") || text.Contains("<a ") || text.Contains("<ul") || text.Contains("<li") || text.Contains("<br"), "no html tag left");
            Check.Eq(false, text.Contains("\n\n\n"), "at most one empty line");
            Check.Eq(string.Empty, HttpHelper.WorkshopRichText(null), "null");
        }

        [TestMethod, Description("HttpHelper.BBCodeRichText: a Workshop description in BBCode -> the same rich text as WorkshopRichText")]
        public void BBCodeRichText_ConvertsBBCode()
        {
            var bb = "[h1]Mod — v1.0[/h1]\r\n[b]Bold[/b] and [i]italic[/i], [url=https://ko-fi.com/x]Ko-fi[/url], [strike]old[/strike]\n\n" +
                     "[h2]⚠ Requirements[/h2]\n[list]\n[*][url=https://a]ModLib[/url] v2.3\n[*][b]Move ModLib[/b] up\n[/list]\n\n\n\n[h3]End[/h3]\n[img]https://x/y.png[/img]last";
            var text = HttpHelper.BBCodeRichText(bb);
            Check.Info(text);
            Check.Eq(true, text.StartsWith("<size=140%><b>Mod — v1.0</b></size>\n<b>Bold</b> and <i>italic</i>, Ko-fi, <s>old</s>"), "h1, b, i, link text, strike");
            Check.Eq(true, text.Contains("<size=120%><b>Requirements</b></size>\n  • ModLib v2.3\n  • <b>Move ModLib</b> up\n"), "h2 without the missing symbol, list items");
            Check.Eq(true, text.EndsWith("<b>End</b>\nlast"), "h3, image dropped");
            Check.Eq(false, text.Contains("[") || text.Contains("\r") || text.Contains("\n\n\n"), "no tag, no CR, at most one empty line");
            Check.Eq(string.Empty, HttpHelper.BBCodeRichText(null), "null");
        }
    }
}
