using ModLib.Attributes;
using System;
using System.Reflection;
using System.Text;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for exception handling and formatting.
    /// Provides utilities to format exception messages with inner exceptions and caller info.
    /// </summary>
    [ActionCatIgn]
    public static class ExceptionHelper
    {
        /// <summary>
        /// Gets formatted string of all inner exceptions with stack traces.
        /// </summary>
        /// <param name="e">Exception</param>
        /// <returns>Formatted exception string</returns>
        public static string GetAllInnnerExceptionStr(this Exception e)
        {
            var f = e;
            var strBuilder = new StringBuilder();
            while (e != null)
            {
                strBuilder.AppendLine(e.Message);
                strBuilder.AppendLine(e.StackTrace);
                e = e.InnerException;
                if (f == e)
                    break;
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// Gets the method that threw the exception from stack trace.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>Calling method</returns>
        public static MethodBase GetCallingMethod(this Exception ex)
        {
            var stackTrace = new System.Diagnostics.StackTrace(ex, true);
            var frame = stackTrace.GetFrame(0); // Get the top-most stack frame
            return frame.GetMethod();
        }
    }
}