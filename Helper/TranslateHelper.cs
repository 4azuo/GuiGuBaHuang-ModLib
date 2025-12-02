using ModLib.Attributes;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ModLib.Helper
{
    [ActionCat("Translate")]
    public static class TranslateHelper
    {
        public const int MAX_LEN = 200;
        public const string TRANS_API = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={2}&tl={1}&dt=t&q={0}";

        public static string Translate(string text, string targetLang, string sourceLang = "auto")
        {
            try
            {
                if (string.IsNullOrEmpty(text) || text.Length > MAX_LEN)
                    return text;
                //DebugHelper.WriteLine(text);

                var url = string.Format(TRANS_API, Encode(text), targetLang, sourceLang);
                //DebugHelper.WriteLine(url);

                var client = new WebClient();
                var res = client.DownloadString(url);

                var result = Decode(ReadGoogleResult(res));
                //DebugHelper.WriteLine(result);
                //DebugHelper.Save();

                return result;
            }
            catch (Exception ex)
            {
                //DebugHelper.WriteLine(ex);
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

        private static readonly Regex r = new Regex("\"(.*?)\",\"(.*?)\"");
        public static string ReadGoogleResult(string input)
        {
            var builder = new StringBuilder();
            foreach (Match m in r.Matches(input))
            {
                if (!m.Groups[2].Value.EndsWith(".md"))
                    builder.Append(m.Groups[1].Value);
            }
            return builder.ToString();
        }
    }
}