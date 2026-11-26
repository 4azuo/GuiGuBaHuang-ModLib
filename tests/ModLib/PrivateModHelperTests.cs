using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>PrivateModHelper: the PRIVATE_MOD constant of the entry class of a mod (no Steam Workshop upload).</summary>
    [TestClass]
    public class PrivateModHelperTests
    {
        private class PrivateEntry { public const bool PRIVATE_MOD = true; }
        private class PublicEntry { public const bool PRIVATE_MOD = false; }
        private class NoConstEntry { }
        private class WrongTypeEntry { public const int PRIVATE_MOD = 1; }
        private class StaticNotConstEntry { public static bool PRIVATE_MOD = true; }

        [TestMethod, Description("ReadPrivateConst: true only for a bool constant set to true")]
        public void ReadPrivateConst_BoolConstantOnly()
        {
            Check.Eq(true, PrivateModHelper.ReadPrivateConst(typeof(PrivateEntry)), "const true");
            Check.Eq(false, PrivateModHelper.ReadPrivateConst(typeof(PublicEntry)), "const false");
            Check.Eq(false, PrivateModHelper.ReadPrivateConst(typeof(NoConstEntry)), "no constant");
            Check.Eq(false, PrivateModHelper.ReadPrivateConst(typeof(WrongTypeEntry)), "not a bool");
            Check.Eq(false, PrivateModHelper.ReadPrivateConst(typeof(StaticNotConstEntry)), "a field, not a constant");
            Check.Eq(false, PrivateModHelper.ReadPrivateConst(null), "no type");
        }

        [TestMethod, Description("CandidateDlls: the installed mod, then debug/Mod_<id> in the project folder and next to it")]
        public void CandidateDlls_Order()
        {
            var rs = PrivateModHelper.CandidateDlls("abc", @"C:\w\123\ModProject", @"C:\m\MOD_abc.dll");
            Check.Eq(3, rs.Count, "3 places");
            Check.Eq(@"C:\m\MOD_abc.dll", rs[0], "installed mod");
            Check.Eq(@"C:\w\123\ModProject\debug\Mod_abc\ModCode\dll\MOD_abc.dll", rs[1], "in the project folder");
            Check.Eq(@"C:\w\123\debug\Mod_abc\ModCode\dll\MOD_abc.dll", rs[2], "next to ModProject (repository layout)");
            Check.Eq(1, PrivateModHelper.CandidateDlls("abc", null, @"C:\m\MOD_abc.dll").Count, "no project");
            Check.Eq(0, PrivateModHelper.CandidateDlls(null, null, null).Count, "nothing");
            Check.Eq("MOD_abc.ModMain", PrivateModHelper.EntryTypeName("abc"), "entry class");
        }

        [TestMethod, Description("FindEntryType: no loaded assembly and no DLL: null (public)")]
        public void FindEntryType_NothingFound()
        {
            Check.Eq(null, PrivateModHelper.FindEntryType("zzNoSuchMod", new[] { @"C:\no\such\MOD_zzNoSuchMod.dll" }), "missing");
        }
    }
}
