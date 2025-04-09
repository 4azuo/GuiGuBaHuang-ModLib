using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class HttpHelper
{
    public static Regex REGEX_WORKSHOP_DESC { get; } = new Regex(@"<div class=""workshopItemDescription"" id=""highlightContent"">(.*?)<\/div>", RegexOptions.Multiline);

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

    public static string DownloadWebContent(string url)
    {
        using (var client = new WebClient())
        {
            return client.DownloadString(url);
        }
    }

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