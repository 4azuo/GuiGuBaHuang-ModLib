using ModLib.Attributes;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Private mods: projects their author does not publish from the game's mod editor (personal, test, in-house mods, or
    /// mods released by their repository). A mod is private with a constant of its entry class:
    ///   namespace MOD_&lt;id&gt; { public class ModMain ... { public const bool PRIVATE_MOD = true; } }
    /// In the mod editor, the Steam Workshop button of its project (UIModMainFunction.btnWorkshop) is hidden and the upload
    /// window (UIModWorkshopUpload) is closed when it opens, with a message (ModUIEvent).
    /// The constant is read from the assembly MOD_&lt;id&gt; already loaded, else from its DLL (the installed mod, then the
    /// build output of the project): a constant is metadata, no code of the mod runs.
    /// It guards against an upload by mistake, not against a copy: anyone can remove it from a DLL.
    /// </summary>
    [ActionCat("PrivateMod")]
    public static class PrivateModHelper
    {
        /// <summary>Name of the constant (bool) of the entry class MOD_&lt;id&gt;.ModMain.</summary>
        public const string CONST_NAME = "PRIVATE_MOD";

        //mod id -> private, read once per opening of the project window
        private static readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Reads the constants again (the project window opens: the author may have rebuilt the mod).</summary>
        public static void ClearCache() { _cache.Clear(); }

        /// <summary>Full name of the entry class of a mod (MOD_&lt;id&gt;.ModMain).</summary>
        public static string EntryTypeName(string modId) => $"MOD_{modId}.ModMain";

        /// <summary>The value of the PRIVATE_MOD constant (bool) of a type; false without it.</summary>
        public static bool ReadPrivateConst(Type entry)
        {
            var f = entry?.GetField(CONST_NAME, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (f == null || !f.IsLiteral || f.FieldType != typeof(bool))
                return false;
            return (bool)f.GetRawConstantValue();
        }

        /// <summary>
        /// The DLLs of a mod that may hold its entry class, first found first: the installed mod
        /// (<paramref name="installedDll"/>), then the build output of the project: debug/Mod_&lt;id&gt;/ModCode/dll next to
        /// the ModProject folder the editor opens (<paramref name="projectPath"/>).
        /// </summary>
        public static List<string> CandidateDlls(string modId, string projectPath, string installedDll)
        {
            var rs = new List<string>();
            if (!string.IsNullOrEmpty(installedDll))
                rs.Add(installedDll);
            if (!string.IsNullOrEmpty(modId) && !string.IsNullOrEmpty(projectPath))
            {
                var dll = Path.Combine("debug", $"Mod_{modId}", "ModCode", "dll", $"MOD_{modId}.dll");
                rs.Add(Path.Combine(projectPath, dll));
                var parent = Path.GetDirectoryName(projectPath.TrimEnd('\\', '/'));
                if (!string.IsNullOrEmpty(parent))
                    rs.Add(Path.Combine(parent, dll));
            }
            return rs;
        }

        /// <summary>The entry class of a mod: in its loaded assembly, else in the first DLL found; null when none.</summary>
        public static Type FindEntryType(string modId, IEnumerable<string> dlls)
        {
            var name = EntryTypeName(modId);
            var asmName = $"MOD_{modId}";
            var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, asmName, StringComparison.OrdinalIgnoreCase));
            if (loaded != null)
                return loaded.GetType(name, false);
            foreach (var f in dlls ?? Enumerable.Empty<string>())
            {
                if (File.Exists(f))
                    return Assembly.LoadFrom(f).GetType(name, false);
            }
            return null;
        }

        /// <summary>A project of the mod editor is private (its PRIVATE_MOD constant is true); errors: public (logged).</summary>
        public static bool IsPrivate(ModProject mod)
        {
            var modId = mod?.projectData?.soleID;
            if (string.IsNullOrEmpty(modId))
                return false;
            if (_cache.TryGetValue(modId, out var cached))
                return cached;
            var rs = false;
            try
            {
                string installed = null;
                try { installed = AssemblyHelper.GetModPathRootAssembly(modId); } catch { /*not installed: the project only*/ }
                rs = ReadPrivateConst(FindEntryType(modId, CandidateDlls(modId, mod.pathProject, installed)));
                if (rs)
                    DebugHelper.WriteLine($"PrivateMod: {modId} is private: Steam Workshop upload hidden");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"PrivateMod: {modId} not read: {ex.Message}");
            }
            _cache[modId] = rs;
            return rs;
        }

        /// <summary>Hides the Steam Workshop button of the project window when its project is private.</summary>
        public static void HideUploadButton(UIModMainFunction ui)
        {
            if (ui == null || !IsPrivate(ui.mod))
                return;
            if (ui.btnWorkshop != null && ui.btnWorkshop.gameObject.activeSelf)
                ui.btnWorkshop.gameObject.SetActive(false);
            if (ui.textWorkshop != null && ui.textWorkshop.gameObject.activeSelf)
                ui.textWorkshop.gameObject.SetActive(false);
        }

        /// <summary>Closes the upload window of a private project, with a message; true when it was closed.</summary>
        public static bool BlockUploadWindow(UIModWorkshopUpload ui)
        {
            if (ui == null || !IsPrivate(ui.mod))
                return false;
            g.ui.CloseUI(ui);
            UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(GameTool.LS("libtxt999990043"), GameTool.LS("libtxt999990044")));
            DebugHelper.WriteLine($"PrivateMod: Steam Workshop upload of {ui.mod?.projectData?.soleID} refused");
            return true;
        }
    }
}
