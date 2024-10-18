using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Exceptions;

namespace ScraperLibrary.COOP
{
    public class COOPAvisScraper : Scraper
    {
        protected async Task<string> GetCurrentAvisExternalId(string url, int timeoutMs = 10000)
        {
            try
            {
                await new BrowserFetcher().DownloadAsync();

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();

                string baseUrl = "https://squid-api.tjek.com/v2/catalogs/";

                var tcs = new TaskCompletionSource<string>();

                page.Request += (sender, e) =>
                {
                    var request = e.Request;

                    if (request.Method == HttpMethod.Get && request.Url.StartsWith(baseUrl))
                    {
                        tcs.TrySetResult(request.Url.Substring(baseUrl.Length));
                    }
                };
                await page.GoToAsync(url);

                var timeoutTask = Task.Delay(timeoutMs);

                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                await browser.CloseAsync();

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"The request was not found within {timeoutMs / 1000} seconds.");
                }

                return await tcs.Task;
            }
            catch (Exception e)
            {
                throw new CannotReachWebsiteException("Failed to get external avis id", e);
            }

        }
    }
}
