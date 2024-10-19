using PuppeteerSharp;

namespace ScraperLibrary
{
    public abstract class Scraper
    {
        protected HttpClient client = new HttpClient();
        public static async Task<string> CallUrl(string fullUrl, int aditionalDelayMs = 0)
        {
            // Download the browser if necessary
            await new BrowserFetcher().DownloadAsync();

            // Randomize user agents
            var random = new Random();
            {
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--disable-web-security", "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
                });

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

                await page.WaitForSelectorAsync("main");

                await Task.Delay(500 + random.Next(500) + aditionalDelayMs);

                var content = await page.GetContentAsync();

                return content;
            }
        }
        protected static dynamic GetInformationFromHtml<T>(string html, string searchPattern, string startSearchKey, string endSearchKey, int startIndexModifier = 0)
        {
            int startIndex = html.IndexOf(searchPattern);
            if (startIndex != -1 && html.Contains(startSearchKey))
            {
                startIndex = html.IndexOf(startSearchKey, startIndex) + startSearchKey.Length + startIndexModifier; // Move past the startSearchKey
                int endIndex = html.IndexOf(endSearchKey, startIndex); // Find the closing tag
                string information = html.Substring(startIndex, endIndex - startIndex).Trim();
                if (typeof(T) == typeof(float)) { return float.Parse(information); }
                else if (typeof(T) == typeof(int)) { return int.Parse(information); }
                else if (typeof(T) == typeof(string)) { return information; }
                else throw new InvalidCastException($"{typeof(T).FullName} is not supported");
            }
            throw new KeyNotFoundException($"searchpattern: {searchPattern} wasn't found");
        }
        protected static DateTime ConvertStringToDate(string date, IFormatProvider originalDateFormat, IFormatProvider newDateFormat)
        {
            DateTime dateTime = DateTime.Parse(date, originalDateFormat);

            DateTime convertedDateTime = DateTime.Parse(dateTime.ToString(newDateFormat), newDateFormat);

            return convertedDateTime;
        }
    }
}