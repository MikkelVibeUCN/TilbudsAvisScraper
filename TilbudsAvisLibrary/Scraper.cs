using PuppeteerSharp;

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
            var userAgents = new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36",
                // Add more user agents if needed
            };
            var random = new Random();
            var randomUserAgent = userAgents[random.Next(userAgents.Length)];

            // Launch a headless browser
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
            });

            // Create a new page
            using var page = await browser.NewPageAsync();

            // Set viewport to a random size to mimic different devices
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1280 + random.Next(0, 100),
                Height = 720 + random.Next(0, 100)
            });

            // Set extra HTTP headers, including randomized user-agent
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                { "user-agent", randomUserAgent },
                { "upgrade-insecure-requests", "1" },
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8" },
                { "accept-encoding", "gzip, deflate, br" },
                { "accept-language", "en-US,en;q=0.9,en;q=0.8" }
            });

            // Spoof navigator.webdriver property
            await page.EvaluateFunctionOnNewDocumentAsync("() => { Object.defineProperty(navigator, 'webdriver', { get: () => false }); }");

            // Block resource-heavy requests
            await page.SetRequestInterceptionAsync(true);
            page.Request += (sender, e) =>
            {
                if (e.Request.ResourceType == ResourceType.Image || e.Request.ResourceType == ResourceType.StyleSheet || e.Request.ResourceType == ResourceType.Font)
                    e.Request.AbortAsync();
                else
                    e.Request.ContinueAsync();
            };

            // Go to the URL
            await page.GoToAsync(fullUrl);

            // Scroll down to simulate user interaction
            await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)");

            // Add a small delay to mimic human behavior
            await Task.Delay(500 + random.Next(500));

            // Get the HTML content of the page
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
