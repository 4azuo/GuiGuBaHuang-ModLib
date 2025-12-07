using ModLib.Attributes;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for debug logging and error tracking.
    /// Manages log file creation, writing, and exception handling with timestamps.
    /// </summary>
    [ActionCat("Debug")]
    public static class DebugHelper
    {
        /// <summary>
        /// Current log buffer that accumulates messages before saving to file.
        /// </summary>
        public static readonly StringBuilder CurLog = new StringBuilder();
        
        /// <summary>
        /// Collection of all exceptions that have been logged.
        /// </summary>
        public static readonly IList<Exception> Exceptions = new List<Exception>();

        /// <summary>
        /// Gets or sets whether debug mode is enabled. When true, shows exception dialogs.
        /// </summary>
        public static bool IsDebugMode { get; set; } = true;

        /// <summary>
        /// Gets the debug log filename with date and player ID.
        /// </summary>
        /// <returns>The debug log filename</returns>
        public static string GetDebugFileName()
        {
            if (GameHelper.IsInGame())
            {
                return $"{g.world.playerUnit.GetUnitId()}_debug-{DateTime.Now:yyyyMMdd}.log";
            }
            else
            {
                return $"debug-{DateTime.Now:yyyyMMdd}.log"; //temp
            }
        }

        /// <summary>
        /// Gets the debug log folder path in LocalApplicationData.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        /// <returns>Path to the debug log folder</returns>
        public static string GetDebugFolderName()
        {
            var p = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModId}\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the full path to the debug log file.
        /// </summary>
        /// <returns>Complete path to the debug log file</returns>
        public static string GetDebugFilePath()
        {
            return Path.Combine(GetDebugFolderName(), GetDebugFileName());
        }

        /// <summary>
        /// Saves the current log buffer to disk and clears it.
        /// </summary>
        public static void Save()
        {
            if (CurLog.Length > 0)
            {
                File.AppendAllText(GetDebugFilePath(), CurLog.ToString());
                CurLog.Clear();
            }
        }

        /// <summary>
        /// Writes a message at a specific position in the log buffer.
        /// </summary>
        /// <param name="pos">Position to insert the message</param>
        /// <param name="msg">Message to write</param>
        /// <returns>The new length of the log buffer</returns>
        public static int WriteAt(int pos, string msg = "")
        {
            CurLog.Insert(pos, msg);
            return CurLog.Length;
        }

        /// <summary>
        /// Writes a message to the log buffer with a newline.
        /// </summary>
        /// <param name="msg">Message to write</param>
        /// <param name="addTime">Whether to prepend timestamp (default: true)</param>
        /// <returns>Position before the newline</returns>
        public static int WriteLine(string msg = "", bool addTime = true)
        {
            if (addTime && !string.IsNullOrEmpty(msg))
                msg = $"{Now()} {msg}";
            CurLog.Append(msg);
            var lastPos = CurLog.Length;
            CurLog.AppendLine();
            return lastPos;
        }

        /// <summary>
        /// Writes a message to the log buffer without a newline.
        /// </summary>
        /// <param name="msg">Message to write</param>
        /// <param name="addTime">Whether to prepend timestamp (default: true)</param>
        /// <returns>The new length of the log buffer</returns>
        public static int Write(string msg = "", bool addTime = true)
        {
            if (addTime && !string.IsNullOrEmpty(msg))
                msg = $"{Now()} {msg}";
            CurLog.Append(msg);
            return CurLog.Length;
        }

        /// <summary>
        /// Logs an exception with caller information and saves immediately.
        /// Shows exception dialog if debug mode is enabled.
        /// </summary>
        /// <param name="e">The exception to log</param>
        /// <param name="memberName">Calling member name (auto-populated)</param>
        /// <param name="filePath">Calling file path (auto-populated)</param>
        /// <param name="lineNumber">Calling line number (auto-populated)</param>
        public static void WriteLine(Exception e,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
        {
            if (Exceptions.Contains(e))
                return;
            Exceptions.Add(e);
            WriteLine($"ModLib-Version: {ModMaster.ModObj.Version}");
            WriteLine($"Caller-Info: {filePath}:{memberName}:(line {lineNumber})");
            WriteLine($"{e.GetAllInnnerExceptionStr()}");
            Save();

            if (IsDebugMode)
                ModMaster.ShowException(e, GetDebugFilePath());
        }

        /// <summary>
        /// Gets the current timestamp string with optional in-game date.
        /// </summary>
        /// <returns>Formatted timestamp, includes game date if in game</returns>
        private static string Now()
        {
            if (GameHelper.IsInGame())
                return $"[{DateTime.Now:HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})]";
            return $"[{DateTime.Now:HH:mm:ss.fff}]";
        }
    }
}