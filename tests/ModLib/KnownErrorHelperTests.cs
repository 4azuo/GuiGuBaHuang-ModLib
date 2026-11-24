using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.IO;

namespace UnitTests.ModLib
{
    /// <summary>KnownErrorHelper: errors the player can fix (reason shown instead of the stack trace).</summary>
    [TestClass]
    public class KnownErrorHelperTests
    {
        [TestMethod, Description("KnownErrorHelper.GetModFolder: the folder before ModConf / ModImg / ModCode, null for another path")]
        public void GetModFolder_FindsModFolder()
        {
            Check.Eq(@"E:\Game\ModExportData\Mod_nE7UL2", KnownErrorHelper.GetModFolder(@"E:\Game\ModExportData\Mod_nE7UL2\ModConf\mod_configs.json"), "file of ModConf");
            Check.Eq(@"E:\Game\ModExportData\Mod_nE7UL2", KnownErrorHelper.GetModFolder(@"E:\Game\ModExportData\Mod_nE7UL2\modconf"), "ModConf folder itself, any case");
            Check.Eq(@"C:\w\3161035078", KnownErrorHelper.GetModFolder("C:/w/3161035078/ModImg/a.png"), "'/' separators");
            Check.Eq(null, KnownErrorHelper.GetModFolder(@"C:\Users\x\AppData\save.json"), "not a mod file");
            Check.Eq(null, KnownErrorHelper.GetModFolder(null), "null");
        }

        [TestMethod, Description("KnownErrorHelper.Find: missing file of a mod folder in the inner exceptions (message of Mono: path in double quotes) -> ModFileMissing")]
        public void Find_MissingModFile()
        {
            var path = @"E:\Online Games\QCBH\ModExportData\Mod_nE7UL2\ModConf\mod_configs.json";
            var e = new Exception("【Error】CallEvents<Il2CppObjectBase>(OnLoadGlobals)",
                new Exception("Exception has been thrown by the target of an invocation.",
                    new DirectoryNotFoundException($"Could not find a part of the path \"{path}\".")));
            var k = KnownErrorHelper.Find(e);
            Check.Eq(KnownErrorHelper.KnownErrorKind.ModFileMissing, k?.Kind, "kind");
            Check.Eq(path, k.Path, "path");
            Check.Eq(@"E:\Online Games\QCBH\ModExportData\Mod_nE7UL2", k.ModFolder, "mod folder");
            var reason = KnownErrorHelper.GetReason(k);
            Check.Eq(true, reason.Contains(path) && reason.Contains(k.ModFolder), "reason names the file and the folder (English fallback without the game)");

            var f = KnownErrorHelper.Find(new FileNotFoundException("Could not find file", @"C:\w\Mod_x\ModImg\a.png"));
            Check.Eq(@"C:\w\Mod_x", f?.ModFolder, "FileNotFoundException.FileName");
        }

        [TestMethod, Description("KnownErrorHelper.Find: ModUserException wins (its message is the reason); other errors are unknown (null)")]
        public void Find_UserErrorAndUnknown()
        {
            var e = new Exception("type initializer", KnownErrorHelper.ConfigMissing("roleCreateFeature", "420010100",
                new DirectoryNotFoundException(@"Could not find a part of the path 'C:\m\Mod_x\ModConf\a.json'.")));
            var k = KnownErrorHelper.Find(e);
            Check.Eq(KnownErrorHelper.KnownErrorKind.UserError, k?.Kind, "user error first");
            Check.Eq(true, KnownErrorHelper.GetReason(k).Contains("roleCreateFeature 420010100"), "config and id in the reason");

            Check.Eq(null, KnownErrorHelper.Find(new NullReferenceException()), "bug: unknown");
            Check.Eq(null, KnownErrorHelper.Find(new FileNotFoundException("x", @"C:\saves\a.sav")), "missing file outside a mod folder: unknown");
            Check.Eq(null, KnownErrorHelper.Find(null), "null");
            Check.Eq("why", KnownErrorHelper.Find(new ModUserException("why"))?.Reason, "own message");
        }

        [TestMethod, Description("KnownErrorHelper.Find: a JSON error inside InConfFile -> ConfFileInvalid with the file; a JSON error without the file or another error in the file: unknown")]
        public void Find_ConfFileInvalid()
        {
            var path = @"C:\m\Mod_x\ModConf\mod_configs.json";
            Exception json = null;
            try { Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, int>>("{ \"a\": 1, "); }
            catch (Exception ex) { json = ex; }
            Check.Eq(true, json is Newtonsoft.Json.JsonException, "Newtonsoft throws a JsonException");

            var k = KnownErrorHelper.Find(new Exception("load", KnownErrorHelper.InConfFile(path, json)));
            Check.Eq(KnownErrorHelper.KnownErrorKind.ConfFileInvalid, k?.Kind, "kind");
            Check.Eq(path, k.Path, "file");
            var reason = KnownErrorHelper.GetReason(k);
            Check.Eq(true, reason.Contains(path) && reason.Contains(json.Message), "reason: file and JSON error");

            Check.Eq(null, KnownErrorHelper.Find(json), "JSON error without the file: unknown");
            Check.Eq(null, KnownErrorHelper.Find(KnownErrorHelper.InConfFile(path, new NullReferenceException())), "bug while loading a file: unknown");
        }

        [TestMethod, Description("KnownErrorHelper.Find: missing member / type of ModLib -> ModLibOutdated, of another assembly -> GameVersionMismatch; other MissingMemberExceptions (bugs): unknown")]
        public void Find_MissingMember()
        {
            var k = KnownErrorHelper.Find(new TypeInitializationException("MOD_x.Enum.E",
                new MissingMethodException("Method not found: void ModLib.Helper.UnitHelper.Foo(int)")));
            Check.Eq(KnownErrorHelper.KnownErrorKind.ModLibOutdated, k?.Kind, "member of ModLib");
            Check.Eq("void ModLib.Helper.UnitHelper.Foo(int)", k.Reason, "missing member");
            Check.Eq(true, KnownErrorHelper.GetReason(k).Contains("void ModLib.Helper.UnitHelper.Foo(int)"), "reason names it (version '?' without the game)");

            var t = KnownErrorHelper.Find(new TypeLoadException("Could not load type 'ModLib.Helper.NewHelper' from assembly 'ModLib, Version=2.2.30.0'."));
            Check.Eq(KnownErrorHelper.KnownErrorKind.ModLibOutdated, t?.Kind, "type of ModLib");

            var g = KnownErrorHelper.Find(new MissingFieldException("Field not found: 'WorldUnitData.oldField'."));
            Check.Eq(KnownErrorHelper.KnownErrorKind.GameVersionMismatch, g?.Kind, "member of the game");
            Check.Eq("WorldUnitData.oldField", g.Reason, "quotes and dot removed");

            Check.Eq(null, KnownErrorHelper.Find(new MissingMethodException("No parameterless constructor defined for this object.")), "Activator bug: unknown");
        }
    }
}
