using ModLib.Attributes;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModLib.Helper
{
    /// <summary>
    /// Private mods: projects their author does not publish (personal, test, in-house mods). A mod is private with a
    /// ModConf/private_mod.json:
    ///   { "Private": true, "Reason": "test mod of the team" }
    /// In the game's mod editor, the Steam Workshop button of its project (UIModMainFunction.btnWorkshop) is hidden and the
    /// upload window (UIModWorkshopUpload) is closed when it opens, with a message (ModUIEvent).
    /// The file is looked for in the project (ModProject/ModConf, the folder the editor opens) and in the installed mod of
    /// the same id (debug/Mod_&lt;id&gt;/ModConf, copied by the build).
    /// </summary>
    [ActionCat("PrivateMod")]
    public static class PrivateModHelper
    {
        /// <summary>Settings file of a mod, in its ModConf folder.</summary>
        public const string CONF_FILE = "private_mod.json";

        /// <summary>Settings of a mod (ModConf/private_mod.json).</summary>
        public class Settings
        {
            /// <summary>The mod is not published (false: the file is ignored).</summary>
            public bool Private { get; set; } = true;
            /// <summary>Why, shown to the author when the upload is refused (optional).</summary>
            public string Reason { get; set; }
        }

        //project path -> settings (null: public), read once per opening of the project window
        private static readonly Dictionary<string, Settings> _cache = new Dictionary<string, Settings>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Reads the private_mod.json files again (the project window opens: the author may have changed them).</summary>
        public static void ClearCache() { _cache.Clear(); }

        /// <summary>
        /// The settings of a private_mod.json text; null when it does not make the mod private (empty, "Private": false,
        /// broken: logged by the caller).
        /// </summary>
        public static Settings ParseSettings(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            var s = JsonConvert.DeserializeObject<Settings>(json);
            if (s == null || !s.Private)
                return null;
            s.Reason = string.IsNullOrWhiteSpace(s.Reason) ? null : s.Reason.Trim();
            return s;
        }

        /// <summary>
        /// Where the private_mod.json of a project may be, first found first: the ModConf of the project folder (the
        /// folder the editor opens is the ModProject folder; its parent also tried), then the ModConf of the installed mod
        /// of the same id (<paramref name="installedConfFolder"/>, null when the mod is not installed).
        /// </summary>
        public static List<string> CandidateFiles(string projectPath, string installedConfFolder)
        {
            var rs = new List<string>();
            if (!string.IsNullOrEmpty(projectPath))
            {
                rs.Add(Path.Combine(projectPath, "ModConf", CONF_FILE));
                rs.Add(Path.Combine(projectPath, "ModProject", "ModConf", CONF_FILE));
            }
            if (!string.IsNullOrEmpty(installedConfFolder))
                rs.Add(Path.Combine(installedConfFolder, CONF_FILE));
            return rs;
        }

        /// <summary>The settings of the first private_mod.json found among <paramref name="files"/>; null: public.</summary>
        public static Settings ReadFirst(IEnumerable<string> files)
        {
            foreach (var f in files)
            {
                if (!File.Exists(f))
                    continue;
                try
                {
                    return ParseSettings(File.ReadAllText(f));
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"PrivateMod: {f} invalid: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        /// <summary>The settings of a project of the mod editor; null: public (no file, or an error: logged).</summary>
        public static Settings GetSettings(ModProject mod)
        {
            try
            {
                var path = mod?.pathProject;
                if (string.IsNullOrEmpty(path))
                    return null;
                if (_cache.TryGetValue(path, out var cached))
                    return cached;
                string installed = null;
                var soleId = mod.projectData?.soleID;
                if (!string.IsNullOrEmpty(soleId))
                {
                    try
                    {
                        installed = ConfHelper.GetConfFolderPath(soleId);
                    }
                    catch
                    {
                        //the mod is not installed: only the project folder
                    }
                }
                var s = ReadFirst(CandidateFiles(path, installed));
                _cache[path] = s;
                if (s != null)
                    DebugHelper.WriteLine($"PrivateMod: project {soleId} ({path}) is private: Steam Workshop upload hidden");
                return s;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                return null;
            }
        }

        /// <summary>A project of the mod editor is private (its Steam Workshop upload is hidden).</summary>
        public static bool IsPrivate(ModProject mod) => GetSettings(mod) != null;

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
            var s = ui == null ? null : GetSettings(ui.mod);
            if (s == null)
                return false;
            g.ui.CloseUI(ui);
            var msg = GameTool.LS("libtxt999990044");
            if (s.Reason != null)
                msg += "\n\n" + s.Reason;
            UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(GameTool.LS("libtxt999990043"), msg));
            DebugHelper.WriteLine($"PrivateMod: Steam Workshop upload of {ui.mod?.projectData?.soleID} refused");
            return true;
        }
    }
}
