using System;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public static class TranslateHelper
{
    public const int MAX_TRANS_LEN = 4500;
    public const string GOOGLE_URL = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={2}&tl={1}&dt=t&q={0}";

    public static string Translate(string text, string targetLang, string sourceLang = "auto")
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var webClient = new WebClient();
        webClient.Encoding = Encoding.UTF8;

        var builder = new StringBuilder();
        var encodeStr = Encode(text);
        for (int i = 0; true; i++)
        {
            var startIndex = MAX_TRANS_LEN * i;
            if (startIndex >= encodeStr.Length)
                break;
            var len = (MAX_TRANS_LEN * (i + 1)).FixValue(0, encodeStr.Length - MAX_TRANS_LEN * i);
            var url = string.Format(GOOGLE_URL, encodeStr.Substring(startIndex, len), targetLang, sourceLang);

            builder.Append(webClient.DownloadString(url));
        }

        var result = builder.ToString();

        //var url = string.Format(GOOGLE_URL, encodeStr, targetLang, sourceLang);
        //var result = webClient.DownloadString(url);

        try
        {
            var translatedText = Decode(result.Substring(4, result.IndexOf("\",\"", 4, StringComparison.Ordinal) - 4));
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
