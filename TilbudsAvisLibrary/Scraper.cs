﻿using PuppeteerSharp;

namespace TilbudsAvisLibrary
{
    public class Scraper
    {
        public Scraper()
        {

        }
        public static async Task<string> CallUrl(string fullUrl)
        {
            // Download the browser if necessary
            await new BrowserFetcher().DownloadAsync();

            // Randomize user agents
            var random = new Random();

            // Launch a headless browser
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-web-security", "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
            });

            // Create a new page
            using var page = await browser.NewPageAsync();

            // Set extra HTTP headers, including randomized user-agent
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36" },
                { "upgrade-insecure-requests", "1" },
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8" },
                { "accept-encoding", "gzip, deflate, br" },
                { "accept-language", "en-US,en;q=0.9,en;q=0.8" }
            });

            await page.GoToAsync(fullUrl);

            await Task.Delay(500 + random.Next(500));

            var content = await page.GetContentAsync();

            return content;
        }

        public static async Task<string> FindAvisUrl(string url)
        {
            var response = await Scraper.CallUrl(url);

            string searchString = "href=\"/avis/";
            int startIndex = response.IndexOf(searchString) + searchString.Length;
            int endIndex = response.IndexOf("\"", startIndex);

            string substring = response.Substring(startIndex, endIndex - startIndex);

            return url + "/" + substring + "/1";
        }
    }
}