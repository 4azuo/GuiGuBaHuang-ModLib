using ModLib.Attributes;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ModLib.Helper
{
    /// <summary>
    /// Errors with a known cause the player can fix himself: the exception dialog shows the reason and what to do instead
    /// of the stack trace (the log keeps the stack), and they are not sent as error reports.
    /// Known errors (first found in the exception and its inner exceptions, in this order):
    /// - ModUserException: its message (thrown by a mod, e.g. ConfigMissing);
    /// - FileNotFoundException / DirectoryNotFoundException of a file in the ModConf / ModImg / ModCode folder of a mod:
    ///   the mod folder is incomplete (bad manual install, broken Workshop download);
    /// - a JSON error (Newtonsoft JsonException) inside a ModConfFileException (InConfFile): a broken config file;
    /// - MissingMemberException (MissingMethod / MissingField) / TypeLoadException: of ModLib = ModLib older than the
    ///   mod needs; of another assembly = game version not matching the mods.
    /// The texts have an English fallback: when the ModConf of ModLib itself is missing, its localTexts are not loaded.
    /// </summary>
    [ActionCatIgn]
    public static class KnownErrorHelper
    {
        /// <summary>Kind of a known error.</summary>
        public enum KnownErrorKind
        {
            /// <summary>ModUserException: the reason is its message.</summary>
            UserError,
            /// <summary>A file of a mod folder is missing.</summary>
            ModFileMissing,
            /// <summary>A config file of a mod is not valid JSON.</summary>
            ConfFileInvalid,
            /// <summary>A member / type of ModLib is missing: ModLib is older than the mod needs.</summary>
            ModLibOutdated,
            /// <summary>A member / type of the game (or of another mod) is missing: versions do not match.</summary>
            GameVersionMismatch,
        }

        /// <summary>A known error found in an exception.</summary>
        public class KnownError
        {
            /// <summary>Kind of error.</summary>
            public KnownErrorKind Kind { get; set; }
            /// <summary>UserError: the reason shown. ConfFileInvalid: the JSON error. ModLibOutdated / GameVersionMismatch: the missing member.</summary>
            public string Reason { get; set; }
            /// <summary>ModFileMissing: the missing file / folder. ConfFileInvalid: the broken file.</summary>
            public string Path { get; set; }
            /// <summary>ModFileMissing: the mod folder (parent of ModConf / ModImg / ModCode).</summary>
            public string ModFolder { get; set; }
        }

        /// <summary>Folders of a mod whose files come with the mod (a missing one = incomplete install).</summary>
        public static readonly string[] MOD_FOLDERS = { "ModConf", "ModImg", "ModCode" };

        /// <summary>Name of the ModLib assembly / namespace in the message of a missing member.</summary>
        public const string MODLIB_NAME = "ModLib";

        private static readonly Regex QuotedPath = new Regex("[\"']([^\"']+)[\"']");

        private const string TXT_TITLE = "libtxt999990025";
        private const string TXT_FILE_MISSING = "libtxt999990026";
        private const string TXT_CONFIG_MISSING = "libtxt999990027";
        private const string TXT_DETAILS = "libtxt999990028";
        private const string TXT_GAME_MISMATCH = "libtxt999990029";
        private const string TXT_MODLIB_OUTDATED = "libtxt999990030";
        private const string TXT_CONF_INVALID = "libtxt999990031";

        private const string EN_TITLE = "Error";
        private const string EN_FILE_MISSING = "Files of a mod are missing:\n{0}\n\nThe folder of this mod is incomplete:\n{1}\n\nReinstall the mod:\n- Steam Workshop: unsubscribe, then subscribe again.\n- Manual install: close the game, delete this folder and copy again the whole Mod_xxx folder of the zip (with ModCode, ModConf and ModImg).";
        private const string EN_CONFIG_MISSING = "A config of a mod was not loaded: {0} {1}.\n\nThe ModConf folder of the mod is missing or the game did not load it.\nReinstall the mod (Steam Workshop: unsubscribe, then subscribe again; manual install: copy again the whole Mod_xxx folder of the zip) and check that your game version is supported by the mod.";
        private const string EN_DETAILS = "Technical details are in the mod log:\n{0}";
        private const string EN_GAME_MISMATCH = "The game version does not match the mods: {0} is missing.\n\n- If the game was just updated: start the game again (MelonLoader rebuilds its files), then update the mods (Steam Workshop updates them by itself; manual install: download the latest versions). If the error stays, wait for an update of the mod.\n- Otherwise: update the game and the mods to their latest versions.";
        private const string EN_MODLIB_OUTDATED = "ModLib is too old for a mod: {0} is missing (installed ModLib: {1}).\n\nUpdate ModLib:\n- Steam Workshop: unsubscribe from ModLib, then subscribe again.\n- Manual install: close the game, delete the old Mod_flQrxq folder and copy the one of the latest ModLib release.";
        private const string EN_CONF_INVALID = "A config file of a mod is damaged:\n{0}\n\n{1}\n\nReinstall the mod (Steam Workshop: unsubscribe, then subscribe again; manual install: copy again the whole Mod_xxx folder of the zip). If you edited this file yourself, fix it or delete your changes.";

        /// <summary>
        /// The known error of an exception (itself or an inner exception), null when the cause is unknown (a bug).
        /// Order: ModUserException, missing mod file, broken config file, missing member / type.
        /// </summary>
        public static KnownError Find(Exception e)
        {
            var chain = new List<Exception>();
            for (var x = e; x != null && !chain.Contains(x); x = x.InnerException)
                chain.Add(x);

            foreach (var x in chain)
            {
                if (x is ModUserException)
                    return new KnownError { Kind = KnownErrorKind.UserError, Reason = x.Message };
            }
            foreach (var x in chain)
            {
                if (!(x is FileNotFoundException) && !(x is DirectoryNotFoundException))
                    continue;
                var path = (x as FileNotFoundException)?.FileName;
                if (string.IsNullOrEmpty(path))
                {
                    var m = QuotedPath.Match(x.Message ?? string.Empty);
                    path = m.Success ? m.Groups[1].Value : null;
                }
                var folder = GetModFolder(path);
                if (folder != null)
                    return new KnownError { Kind = KnownErrorKind.ModFileMissing, Path = path, ModFolder = folder };
            }
            for (var i = 0; i < chain.Count; i++)
            {
                if (!(chain[i] is ModConfFileException cf))
                    continue;
                for (var j = i + 1; j < chain.Count; j++)
                {
                    if (chain[j] is JsonException)
                        return new KnownError { Kind = KnownErrorKind.ConfFileInvalid, Path = cf.FilePath, Reason = chain[j].Message };
                }
            }
            foreach (var x in chain)
            {
                if (!IsMissingMember(x))
                    continue;
                var what = GetMissingName(x);
                var ofModLib = (x.Message ?? string.Empty).IndexOf(MODLIB_NAME, StringComparison.Ordinal) >= 0;
                return new KnownError { Kind = ofModLib ? KnownErrorKind.ModLibOutdated : KnownErrorKind.GameVersionMismatch, Reason = what };
            }
            return null;
        }

        /// <summary>
        /// Messages of a member / type missing at run time (assembly older or newer than the code using it). Other
        /// MissingMemberExceptions are bugs (e.g. "No parameterless constructor defined" of Activator.CreateInstance).
        /// </summary>
        public static readonly string[] MISSING_MEMBER_MESSAGES = { "Method not found", "Field not found", "Could not load type", "Could not resolve type" };

        /// <summary>A member / type missing at run time (MissingMemberException / TypeLoadException with a MISSING_MEMBER_MESSAGES message).</summary>
        public static bool IsMissingMember(Exception x)
        {
            if (!(x is MissingMemberException) && !(x is TypeLoadException))
                return false;
            var msg = x.Message ?? string.Empty;
            foreach (var m in MISSING_MEMBER_MESSAGES)
            {
                if (msg.StartsWith(m, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The missing member / type of a MissingMemberException / TypeLoadException: the part of the message after ':'
        /// ("Method not found: void A.B()"), else the type name, else the message.
        /// </summary>
        public static string GetMissingName(Exception x)
        {
            var msg = x.Message ?? string.Empty;
            var i = msg.IndexOf(": ", StringComparison.Ordinal);
            if (i > 0 && i + 2 < msg.Length)
                return msg.Substring(i + 2).Trim().TrimEnd('.').Trim('\'');
            if (x is TypeLoadException t && !string.IsNullOrEmpty(t.TypeName))
                return t.TypeName;
            return msg;
        }

        /// <summary>
        /// The mod folder of a path inside its ModConf / ModImg / ModCode folder (the part before it), null for another path.
        /// </summary>
        public static string GetModFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var p = path.Replace('/', '\\');
            var best = -1;
            foreach (var f in MOD_FOLDERS)
            {
                var i = p.IndexOf($"\\{f}\\", StringComparison.OrdinalIgnoreCase);
                if (i < 0 && p.EndsWith($"\\{f}", StringComparison.OrdinalIgnoreCase))
                    i = p.Length - f.Length - 1;
                if (i > 0 && (best < 0 || i < best))
                    best = i;
            }
            return best > 0 ? p.Substring(0, best) : null;
        }

        /// <summary>Text shown to the player for a known error (in his language).</summary>
        public static string GetReason(KnownError k)
        {
            switch (k.Kind)
            {
                case KnownErrorKind.ModFileMissing:
                    return string.Format(Text(TXT_FILE_MISSING, EN_FILE_MISSING), k.Path, k.ModFolder);
                case KnownErrorKind.ConfFileInvalid:
                    return string.Format(Text(TXT_CONF_INVALID, EN_CONF_INVALID), k.Path, k.Reason);
                case KnownErrorKind.ModLibOutdated:
                    return string.Format(Text(TXT_MODLIB_OUTDATED, EN_MODLIB_OUTDATED), k.Reason, GetModLibVersion());
                case KnownErrorKind.GameVersionMismatch:
                    return string.Format(Text(TXT_GAME_MISMATCH, EN_GAME_MISMATCH), k.Reason);
                default:
                    return k.Reason;
            }
        }

        /// <summary>Title of the dialog of a known error.</summary>
        public static string GetTitle() => Text(TXT_TITLE, EN_TITLE);

        /// <summary>End of the dialog of a known error: the details are in the log (its path).</summary>
        public static string GetDetailsNote(string log) => string.Format(Text(TXT_DETAILS, EN_DETAILS), log);

        /// <summary>
        /// Error for a game config a mod needs and that is not loaded (e.g. an id of the mod's ModConf):
        /// throw KnownErrorHelper.ConfigMissing("roleCreateFeature", id).
        /// </summary>
        public static ModUserException ConfigMissing(string confName, object id, Exception inner = null)
        {
            return new ModUserException(string.Format(Text(TXT_CONFIG_MISSING, EN_CONFIG_MISSING), confName, id), inner);
        }

        /// <summary>
        /// Wraps an error while reading a config file of a mod, with the file path: a JSON error inside is shown as a broken
        /// file. Use it around the parsing: catch (Exception ex) { throw KnownErrorHelper.InConfFile(path, ex); }
        /// </summary>
        public static ModConfFileException InConfFile(string filePath, Exception inner)
        {
            return new ModConfFileException(filePath, inner);
        }

        /// <summary>A localText of ModLib, the English fallback when it is not loaded (ModConf of ModLib missing).</summary>
        public static string Text(string key, string fallback)
        {
            try
            {
                var s = GameText(key);
                if (!string.IsNullOrEmpty(s) && !s.Contains(key))
                    return s;
            }
            catch
            {
                //no game (unit tests) or text table not ready
            }
            return fallback;
        }

        //own methods: without the game (unit tests), loading the game types fails here, inside the try of the caller
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GameText(string key) => GameTool.LS(key);

        private static string GetModLibVersion()
        {
            try { return ModLibVersion(); }
            catch { return "?"; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string ModLibVersion() => Mod.ModMaster.ModObj?.Version ?? "?";
    }
}
