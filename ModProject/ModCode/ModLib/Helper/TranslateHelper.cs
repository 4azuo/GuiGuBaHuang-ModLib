using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

public static class TranslateHelper
{
    public const string GOOGLE_URL = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={2}&tl={1}&dt=t&q={0}";
    public static WebClient WebClient { get; } = new WebClient()
    {
        Encoding = System.Text.Encoding.UTF8,
    };

    public static string Translate(string text, string targetLang, string sourceLang = "auto")
    {
        var url = string.Format(GOOGLE_URL, Encode(text), targetLang, sourceLang);

        var result = WebClient.DownloadString(url);

        //DebugHelper.WriteLine(result);
        //DebugHelper.Save();

        try
        {
            var translatedText = Decode(result.Substring(4, result.IndexOf("\",\"", 4, StringComparison.Ordinal) - 4));
            //DebugHelper.WriteLine(translatedText);
            //DebugHelper.Save();
            return translatedText;
        }
        catch
        {
            return text;
        }
    }

    public static string Encode(string str)
    {
        return HttpUtility.UrlEncode(str
            .Replace("\r", "/1/")
            .Replace("\n", "/2/")
            .Replace("\t", "/3/")
            .Replace("\\", "/4/")
            .Replace("\"", "/5/"));
    }

    public static string Decode(string str)
    {
        return Regex.Replace(str, @"\\u(?<code>\w{4})", To32bitChar)
            .Replace("/1/", "\r")
            .Replace("/2/", "\n")
            .Replace("/3/", "\t")
            .Replace("/4/", "\\")
            .Replace("/5/", "\"");
    }

    public static string To32bitChar(Match match)
    {
        var code = match.Groups["code"].Value;
        var v = Convert.ToInt32(code, 16);
        return ((char)v).ToString();
    }
}
