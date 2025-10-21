using System;

namespace ModLib.Helper
{
    public partial class ActionHelper
    {
        public static void Error(Exception ex)
        {
            var exMethod = ex?.GetCallingMethod();
            var exType = exMethod.DeclaringType;
            DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
        }
    }
}
