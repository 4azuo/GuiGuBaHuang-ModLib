using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>HitHelper: kinds of the hits made by code (pure part; the hits themselves need the game).</summary>
    [TestClass]
    public class HitHelperTests
    {
        [TestMethod, Description("HitHelper.TypesOf: the first of the 12 attributes of the game -> weapon type (blade 1..finger 6) / element (fire 1..wood 6); others skipped; none: 0/0")]
        public void TypesOf_MapsTheTwelveAttributes()
        {
            Check.Eq(12, HitHelper.ATTRIBUTES.Length, "12 attributes");
            for (var i = 0; i < 6; i++)
            {
                var w = HitHelper.TypesOf(HitHelper.ATTRIBUTES[i]);
                Check.Eq(i + 1, w.Key, $"{HitHelper.ATTRIBUTES[i]}: weapon type");
                Check.Eq(0, w.Value, $"{HitHelper.ATTRIBUTES[i]}: no element");
                var m = HitHelper.TypesOf(HitHelper.ATTRIBUTES[6 + i]);
                Check.Eq(0, m.Key, $"{HitHelper.ATTRIBUTES[6 + i]}: no weapon type");
                Check.Eq(i + 1, m.Value, $"{HitHelper.ATTRIBUTES[6 + i]}: element");
            }
            Check.Eq(2, HitHelper.TypesOf("Froze").Value, "froze (name of the game) = frost, any case");
            Check.Eq(5, HitHelper.TypesOf(new[] { "none", "light", null, "earth", "sword" }).Value, "first attribute of the game wins (earth)");
            Check.Eq(0, HitHelper.TypesOf(new[] { "light", "dark" }).Key + HitHelper.TypesOf(new[] { "light", "dark" }).Value, "none of the game: 0/0");
            Check.Eq(0, HitHelper.TypesOf((string[])null).Value, "null: 0/0");
        }

        [TestMethod, Description("HitHelper.SafeTypes: never 0/0 (the game throws on a hit of no kind): its own kind, else the kind of its martial art (weapon first), else fire")]
        public void SafeTypes_NeverNoKind()
        {
            var t = HitHelper.SafeTypes(3, 0, 1, 2);
            Check.Eq(3, t.Key, "own weapon type kept"); Check.Eq(0, t.Value, "own weapon type: no element");
            t = HitHelper.SafeTypes(0, 4, 1, 0);
            Check.Eq(0, t.Key, "own element kept"); Check.Eq(4, t.Value, "own element");
            t = HitHelper.SafeTypes(0, 0, 5, 3);
            Check.Eq(5, t.Key, "no kind: weapon type of the martial art"); Check.Eq(0, t.Value, "weapon type only");
            t = HitHelper.SafeTypes(0, 0, 0, 3);
            Check.Eq(0, t.Key, "no kind: element of the martial art"); Check.Eq(3, t.Value, "element of the martial art");
            t = HitHelper.SafeTypes(0, 0);
            Check.Eq(0, t.Key, "nothing: no weapon"); Check.Eq(HitHelper.DEFAULT_MAGIC, t.Value, "nothing: fire");
            t = HitHelper.SafeTypes(-1, -2);
            Check.Eq(HitHelper.DEFAULT_MAGIC, t.Value, "negative values: no kind -> fire");
        }
    }
}
