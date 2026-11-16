using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// Tests that reproduce the bugs listed in docs/Known-Issues.md. They FAIL until the bug is fixed and are
    /// excluded by default (scripts\run-unittests.ps1 -IncludeKnownIssues runs them). When a bug is fixed,
    /// move its test to the normal tests and remove its entry from the allow-lists below.
    /// </summary>
    [TestClass]
    public class KnownIssues
    {
        /// <summary>Events without entry in _EventOrderIndex.json, tolerated by ConfigTests (none: BUG-09 fixed).</summary>
        public static readonly string[] EventsWithoutOrder = { };

        /// <summary>GameTool.LS keys used in code but missing in LocalText, tolerated by ConfigTests (none: BUG-08 fixed).</summary>
        public static readonly string[] MissingLocalTextKeys = { };

        // no open bug reproduced here: BUG-07/08/09 fixed (their checks are in ObjectTests.PngEncode_Signature and
        // ConfigTests.EventOrderIndex_MatchesEvents / LocalTextKeys_UsedInCode_Exist)
    }
}
