using ModLib.Attributes;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for HTTP operations and web content downloading.
    /// Provides utilities for downloading web content and parsing Steam Workshop descriptions.
    /// </summary>
    [ActionCat("Http")]
    public static class HttpHelper
    {
        /// <summary>
        /// Regex for extracting Steam Workshop item description (stops at the first inner &lt;/div&gt;: the headings of
        /// the description are divs).
        /// </summary>
        [System.Obsolete("Stops at the first heading of the description: use ExtractWorkshopDescription / GetWorkshopDescription.")]
        public static Regex REGEX_WORKSHOP_DESC { get; } = new Regex(@"<div class=""workshopItemDescription"" id=""highlightContent"">(.*?)<\/div>", RegexOptions.Multiline);

        /// <summary>
        /// Downloads web content asynchronously.
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <returns>Content string or empty on error</returns>
        public static async Task<string> DownloadWebContentAsync(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Downloads web content synchronously with timeout.
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <returns>Content string or empty on error</returns>
        public static string DownloadWebContent(string url)
        {
            using (var client = new WebClientWithTimeout() { Timeout = 10000 })
            {
                try
                {
                    return client.DownloadString(url);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Downloads a Steam Workshop item page and returns its description as TextMeshPro rich text (WorkshopRichText).
        /// </summary>
        /// <param name="url">Workshop URL</param>
        /// <returns>Description (rich text) or empty string</returns>
        public static string GetWorkshopDescription(string url)
        {
            return WorkshopRichText(ExtractWorkshopDescription(DownloadWebContent(url)));
        }

        private const string WORKSHOP_DESC_START = "<div class=\"workshopItemDescription\" id=\"highlightContent\">";
        private static readonly Regex DIV_TAG = new Regex(@"<div\b|</div\s*>", RegexOptions.IgnoreCase);

        /// <summary>
        /// The HTML inside the description block of a Workshop page (its nested divs included: the headings of the
        /// description are divs too), empty when not found.
        /// </summary>
        public static string ExtractWorkshopDescription(string page)
        {
            if (string.IsNullOrEmpty(page)) return string.Empty;
            var start = page.IndexOf(WORKSHOP_DESC_START, System.StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            start += WORKSHOP_DESC_START.Length;
            var depth = 1;
            foreach (Match m in DIV_TAG.Matches(page, start))
            {
                depth += m.Value.StartsWith("</") ? -1 : 1;
                if (depth == 0) return page.Substring(start, m.Index - start);
            }
            return page.Substring(start);
        }

        private static readonly System.Tuple<Regex, string>[] HTML_TO_RICH =
        {
            System.Tuple.Create(new Regex(@"\r?\n"), string.Empty),                                                       // the page's own line breaks mean nothing
            System.Tuple.Create(new Regex(@"<div class=""bb_h1"">(.*?)</div>", RegexOptions.IgnoreCase), "<size=140%><b>$1</b></size>\n"),
            System.Tuple.Create(new Regex(@"<div class=""bb_h2"">(.*?)</div>", RegexOptions.IgnoreCase), "\n<size=120%><b>$1</b></size>\n"),
            System.Tuple.Create(new Regex(@"<div class=""bb_h3"">(.*?)</div>", RegexOptions.IgnoreCase), "\n<b>$1</b>\n"),
            System.Tuple.Create(new Regex(@"<span class=""bb_link_host"">.*?</span>", RegexOptions.IgnoreCase), string.Empty), // "[github.com]" after an external link
            System.Tuple.Create(new Regex(@"<br\s*/?>\s*</li>", RegexOptions.IgnoreCase), "</li>"),                    // "<br></li>": one line break
            System.Tuple.Create(new Regex(@"<li\b[^>]*>", RegexOptions.IgnoreCase), "  • "),
            System.Tuple.Create(new Regex(@"</li>", RegexOptions.IgnoreCase), "\n"),
            System.Tuple.Create(new Regex(@"<br\s*/?>", RegexOptions.IgnoreCase), "\n"),
            System.Tuple.Create(new Regex(@"<(/?)(?:strike|del)>", RegexOptions.IgnoreCase), "<$1s>"),
            System.Tuple.Create(new Regex(@"<(?!/?(?:b|i|u|s)>|size=|/size>)[^>]+>", RegexOptions.IgnoreCase), string.Empty), // other tags (div, ul, a, img, span...); the rich text tags above kept
        };

        private static readonly System.Tuple<Regex, string>[] BBCODE_TO_RICH =
        {
            System.Tuple.Create(new Regex(@"\r\n?"), "\n"),
            System.Tuple.Create(new Regex(@"\[img\].*?\[/img\]", RegexOptions.IgnoreCase | RegexOptions.Singleline), string.Empty),
            System.Tuple.Create(new Regex(@"\[h1\](.*?)\[/h1\]\n?", RegexOptions.IgnoreCase), "<size=140%><b>$1</b></size>\n"),
            System.Tuple.Create(new Regex(@"\[h2\](.*?)\[/h2\]\n?", RegexOptions.IgnoreCase), "\n<size=120%><b>$1</b></size>\n"),
            System.Tuple.Create(new Regex(@"\[h3\](.*?)\[/h3\]\n?", RegexOptions.IgnoreCase), "\n<b>$1</b>\n"),
            System.Tuple.Create(new Regex(@"\[url=[^\]]*\](.*?)\[/url\]", RegexOptions.IgnoreCase), "$1"),
            System.Tuple.Create(new Regex(@"\[url\](.*?)\[/url\]", RegexOptions.IgnoreCase), "$1"),
            System.Tuple.Create(new Regex(@"\[(/?)(?:strike|s)\]", RegexOptions.IgnoreCase), "<$1s>"),
            System.Tuple.Create(new Regex(@"\[(/?)(b|i|u)\]", RegexOptions.IgnoreCase), "<$1$2>"),
            System.Tuple.Create(new Regex(@"\[\*\]\s*", RegexOptions.IgnoreCase), "  • "),
            System.Tuple.Create(new Regex(@"\[/?(?:list|olist|quote|code|noparse|spoiler|hr)\]\n?", RegexOptions.IgnoreCase), string.Empty),
        };

        /// <summary>
        /// Steam BBCode (a Workshop description kept in the mod: ModAssets) -> TextMeshPro rich text, as WorkshopRichText
        /// shows a downloaded one: headings bold and bigger, list items "•", b/i/u/s kept, links reduced to their text,
        /// images and the other tags dropped.
        /// </summary>
        public static string BBCodeRichText(string bbcode)
        {
            if (string.IsNullOrEmpty(bbcode)) return string.Empty;
            var s = bbcode;
            foreach (var r in BBCODE_TO_RICH)
                s = r.Item1.Replace(s, r.Item2);
            // symbols the game's font does not have (shown as a box)
            s = Regex.Replace(s, @"[⚠️]\s*", string.Empty);
            s = Regex.Replace(s, @"[ \t]+\n", "\n");
            s = Regex.Replace(s, @"\n{3,}", "\n\n");
            return s.Trim();
        }

        /// <summary>
        /// Workshop description HTML (Steam's rendering of the BBCode) -> TextMeshPro rich text: headings bold and
        /// bigger, list items "•", line breaks, b/i/u/s kept, links reduced to their text, entities decoded.
        /// </summary>
        public static string WorkshopRichText(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var s = html;
            foreach (var r in HTML_TO_RICH)
                s = r.Item1.Replace(s, r.Item2);
            s = System.Net.WebUtility.HtmlDecode(s);
            // symbols the game's font does not have (shown as a box)
            s = Regex.Replace(s, @"[⚠️]\s*", string.Empty);
            s = Regex.Replace(s, @"[ \t]+\n", "\n");
            s = Regex.Replace(s, @"\n{3,}", "\n\n");
            return s.Trim();
        }
    }
}