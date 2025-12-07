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
        /// Regex for extracting Steam Workshop item description.
        /// </summary>
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
        /// Downloads and parses Steam Workshop item description.
        /// </summary>
        /// <param name="url">Workshop URL</param>
        /// <returns>Description HTML or empty string</returns>
        public static string GetWorkshopDescription(string url)
        {
            var content = DownloadWebContent(url);
            Match m = REGEX_WORKSHOP_DESC.Match(content);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}