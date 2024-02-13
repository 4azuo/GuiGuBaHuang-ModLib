using System;
using System.Text;

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
}