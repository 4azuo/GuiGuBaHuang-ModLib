using ModLib.Attributes;
using System;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for handling actions and lambda function errors.
    /// Provides utilities for traced wrappers and error handling.
    /// </summary>
    [ActionCatIgn]
    public partial class ActionHelper
    {
        /// <summary>
        /// Logs an error from an exception, specifically for lambda function errors.
        /// Extracts the calling method and type information to provide detailed error context.
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        public static void Error(Exception ex)
        {
            var exMethod = ex?.GetCallingMethod();
            var exType = exMethod.DeclaringType;
            DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
        }
    }
}
