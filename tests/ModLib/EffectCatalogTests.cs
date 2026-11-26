using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Object;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.ModLib
{
    /// <summary>EffectCatalog / EffectCatalogItem / SkillIconTags: the catalogs of effects and skill icons a mod reads (pure parts).</summary>
    [TestClass]
    public class EffectCatalogTests
    {
        private static EffectCatalogItem Item(string path, string name, params string[] elements) =>
            new EffectCatalogItem { Kind = path.Split('\\')[2], Path = path, Name = name, Note = "a " + name, Element = elements.ToList() };

        private static EffectCatalog Catalog() => new EffectCatalog(new[]
        {
            Item("Effect\\Battle\\Skill\\Aixin", "Aixin", "fire"),
            Item("Effect\\Battle\\Skill\\Bing", "Bing", "frost", "blade"),
            Item("Effect\\Battle\\Unit\\Lei", "Lei", "thunder"),
        });

        [TestMethod, Description("EffectCatalog.Find: by path, '/' or '\\', any case, spaces around; null when unknown")]
        public void Find_ByPathAnyForm()
        {
            var c = Catalog();
            Check.Eq("Aixin", c.Find("Effect\\Battle\\Skill\\Aixin")?.Name, "exact");
            Check.Eq("Aixin", c.Find(" effect/battle/skill/AIXIN ")?.Name, "slashes, case, spaces");
            Check.Eq(null, c.Find("Effect\\Battle\\Skill\\None"), "unknown");
            Check.Eq(null, c.Find(null), "null");
        }

        [TestMethod, Description("EffectCatalog.Search: kind filter, every word in name / note / elements")]
        public void Search_KindAndWords()
        {
            var c = Catalog();
            Check.Eq(3, c.Search(null, "").Count, "everything");
            Check.Eq(2, c.Search("Skill", null).Count, "a kind");
            Check.Eq(1, c.Search("Skill", "FROST blade").Count, "every word, any case");
            Check.Eq(0, c.Search("Unit", "fire").Count, "word of another kind");
        }

        [TestMethod, Description("EffectCatalogItem: attributes without 'none'; shown 0.5-6 s, 2 s looping or unknown")]
        public void Item_AttributesAndShowSec()
        {
            Check.Seq(new[] { "fire" }, new EffectCatalogItem { Element = new List<string> { "none", "fire" } }.Attributes(), "none left out");
            Check.Eq(0, new EffectCatalogItem { Element = null }.Attributes().Count, "no list");
            Check.Eq(2f, new EffectCatalogItem().ShowSec(), 0.001, "unknown");
            Check.Eq(2f, new EffectCatalogItem { DurationMs = 900, Loop = true }.ShowSec(), 0.001, "looping");
            Check.Eq(0.5f, new EffectCatalogItem { DurationMs = 100 }.ShowSec(), 0.001, "short");
            Check.Eq(6f, new EffectCatalogItem { DurationMs = 90000 }.ShowSec(), 0.001, "long");
            Check.Eq(1.2f, new EffectCatalogItem { DurationMs = 1200 }.ShowSec(), 0.001, "measured");
        }

        [TestMethod, Description("SkillIconTags.Suited: the icons sharing a wanted tag, the best first; all when none suits")]
        public void IconTags_Suited()
        {
            var tags = new SkillIconTags(new Dictionary<string, List<string>>
            {
                ["a"] = new List<string> { "blade", "fire" }, ["b"] = new List<string> { "fire" }, ["c"] = new List<string> { "frost" }, ["d"] = new List<string> { "sword" },
            });
            var icon = new Dictionary<int, string> { [1] = "a", [2] = "b", [3] = "c", [4] = "d" };
            var ids = new List<int> { 1, 2, 3, 4 };
            Check.Seq(new[] { 1, 2 }, tags.Suited(ids, new[] { "fire", "blade" }, b => icon[b]), "fire + blade: 'a' (2 tags) then 'b'");
            Check.Seq(new[] { 3 }, tags.Suited(ids, new[] { "frost" }, b => icon[b]), "frost: the frost icons");
            Check.Seq(new[] { 1, 2, 3, 4 }, tags.Suited(ids, new string[0], b => icon[b]), "nothing wanted: every icon");
            Check.Seq(new[] { 1, 2, 3, 4 }, tags.Suited(ids, null, b => icon[b]), "null: every icon");
            Check.Eq(0, tags.Score("x", new[] { "fire" }), "icon without tags");
        }
    }
}
