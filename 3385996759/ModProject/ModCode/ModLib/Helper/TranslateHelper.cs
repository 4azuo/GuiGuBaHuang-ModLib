using ModLib.Attributes;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for text translation using Google Translate API.
    /// Provides utilities for translating text between languages.
    /// </summary>
    [ActionCat("Translate")]
    public static class TranslateHelper
    {
        public const int MAX_LEN = 200;
        public const string TRANS_API = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={2}&tl={1}&dt=t&q={0}";

        /// <summary>
        /// Translates text using Google Translate API.
        /// </summary>
        /// <param name="text">Text to translate (max 200 chars)</param>
        /// <param name="targetLang">Target language code</param>
        /// <param name="sourceLang">Source language code (auto-detect if "auto")</param>
        /// <returns>Translated text or original on error</returns>
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

        /// <summary>
        /// Encodes string for URL with special character replacement.
        /// </summary>
        /// <param name="str">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string Encode(string str)
        {
            return HttpUtility.UrlEncode(str
                .Replace("\r", "/1/")
                .Replace("\n", "/2/")
                .Replace("\t", "/3/")
                .Replace("\\", "/4/")
                .Replace("\"", "/5/"));
        }

        /// <summary>
        /// Decodes URL-encoded string and restores special characters.
        /// </summary>
        /// <param name="str">Encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string str)
        {
            return Regex.Replace(str, @"\\u(?<code>\w{4})", To32bitChar)
                .Replace("/1/", "\r")
                .Replace("/2/", "\n")
                .Replace("/3/", "\t")
                .Replace("/4/", "\\")
                .Replace("/5/", "\"");
        }

        /// <summary>
        /// Converts Unicode escape sequence to character.
        /// </summary>
        /// <param name="match">Regex match</param>
        /// <returns>Character string</returns>
        public static string To32bitChar(Match match)
        {
            var code = match.Groups["code"].Value;
            var v = Convert.ToInt32(code, 16);
            return ((char)v).ToString();
        }

        private static readonly Regex r = new Regex("\"(.*?)\",\"(.*?)\"");
        /// <summary>
        /// Parses Google Translate API response.
        /// </summary>
        /// <param name="input">API response</param>
        /// <returns>Translated text</returns>
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