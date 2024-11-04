using PuppeteerSharp;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisLibrary.Exceptions;

namespace ScraperLibrary.COOP
{
    public abstract class COOPAvisScraper : Scraper, IAvisScraper
    {
        public string AvisUrl { get; set; }
        private IProductScraper _productScraper;
        protected COOPAvisScraper(string avisUrl, IProductScraper productScraper)
        {
            this.AvisUrl = avisUrl;
            _productScraper = productScraper;
        }
        public Task DownloadAllPagesAsImages(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<string> FindAvisUrl(string url)
        {
            return await GetCurrentAvisExternalId(url);
        }

        public string GetImageUrl(string input, int pageNumber)
        {
            throw new NotImplementedException();
        }

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

        public async Task<Avis> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            string externalAvisId = await FindAvisUrl(AvisUrl);
            progressCallback(23);

            var getDatesTask = await GetAvisDates(AvisUrl, externalAvisId);
            progressCallback(44);

            List<Product> products = await _productScraper.GetAllProductsFromPage(progressCallback, token, externalAvisId, companyId);
            progressCallback(100);

            return new Avis(externalAvisId, getDatesTask.Item1, getDatesTask.Item2, new List<TilbudsAvisLibrary.Entities.Page>(), products);
        }

        private static async Task<(DateTime, DateTime)> GetAvisDates(string url, string externalAvisId)
        {
            IFormatProvider danishDateFormat = new CultureInfo("da-DK");
            IFormatProvider americanDateFormat = new CultureInfo("en-US");

            var response = await CallUrl(url, 10000);

            File.WriteAllText("response.text", response);

            string validFromTo = GetInformationFromHtml<string>(response, "avisen gælder fra", "avisen gælder fra ", "<", 0, true);

            string[] dates = validFromTo.Split("til");

            DateTime validFrom = ConvertStringToDate(dates[0], danishDateFormat, americanDateFormat);
            DateTime validTo = ConvertStringToDate(dates[1], danishDateFormat, americanDateFormat);

            return (validFrom, validTo);
        }
    }
}
