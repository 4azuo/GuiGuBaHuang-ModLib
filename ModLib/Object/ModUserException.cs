using System;

namespace ModLib.Object
{
    /// <summary>
    /// Error with a reason the player can fix himself (bad install, config of a mod not loaded...).
    /// The exception dialog shows its Message (already in the player's language) instead of the stack trace; the log still
    /// gets the stack; no error report is sent. Make one with KnownErrorHelper (e.g. KnownErrorHelper.ConfigMissing).
    /// </summary>
    public class ModUserException : Exception
    {
        /// <param name="reason">Text shown to the player: what is wrong and what to do</param>
        /// <param name="inner">Original error (logged)</param>
        public ModUserException(string reason, Exception inner = null) : base(reason, inner) { }
    }
}
