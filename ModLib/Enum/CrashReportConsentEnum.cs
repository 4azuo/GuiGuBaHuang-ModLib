namespace ModLib.Enum
{
    /// <summary>
    /// The player's answer to "send error reports to the mod author" (ModTitleEvent.CrashReportConsent, global cache).
    /// Nothing is sent unless it is Allowed.
    /// </summary>
    public enum CrashReportConsentEnum
    {
        /// <summary>Not answered yet: asked on the title screen, nothing sent.</summary>
        NotAsked = 0,
        /// <summary>The player allowed the reports.</summary>
        Allowed = 1,
        /// <summary>The player refused: nothing sent.</summary>
        Denied = 2,
    }
}
