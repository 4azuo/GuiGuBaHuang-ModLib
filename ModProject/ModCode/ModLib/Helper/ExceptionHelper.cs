using ModLib.Attributes;
using System;
using System.Reflection;
using System.Text;

namespace ModLib.Helper
{
    [ActionCat("Exception")]
    public static class ExceptionHelper
    {
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

        public static MethodBase GetCallingMethod(this Exception ex)
        {
            var stackTrace = new System.Diagnostics.StackTrace(ex, true);
            var frame = stackTrace.GetFrame(0); // Get the top-most stack frame
            return frame.GetMethod();
        }
    }
}