using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>ActionHelper.TracedFunc: the default given is returned when there is no function or it throws.</summary>
    [TestClass]
    public class ActionHelperTests
    {
        [TestMethod, Description("TracedFunc(null, def) returns def (was default(T): the S&M sliders without a condition scored 0)")]
        public void TracedFunc_NullReturnsDefault()
        {
            Check.True(ActionHelper.TracedFunc<int, bool>(null, true)(1), "no function, default true");
            Check.Eq(7, ActionHelper.TracedFunc<int>(null, 7)(), "no function, default 7");
            Check.Eq(5, ActionHelper.TracedFunc<int, int, int>(null, 5)(1, 2), "2 arguments");
            Check.Eq(3, ActionHelper.TracedFunc<int, int>(x => x + 2, 9)(1), "a function: its result");
        }
    }
}
