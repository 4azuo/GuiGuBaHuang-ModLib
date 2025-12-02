using ModLib.Attributes;
using System;

namespace ModLib.Helper
{
    [ActionCat("Action")]
    public partial class ActionHelper
    {
        [ActionCatIgn]
        public static void Error(Exception ex)
        {
            var exMethod = ex?.GetCallingMethod();
            var exType = exMethod.DeclaringType;
            DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
        }
    }
}
