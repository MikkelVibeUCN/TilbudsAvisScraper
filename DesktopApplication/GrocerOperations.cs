using TilbudsAvisLibrary.DTO;
using ScraperLibrary._365_Discount;
using ScraperLibrary.COOP.Brugsen;
using ScraperLibrary.COOP.Kvickly;
using ScraperLibrary.COOP.SuperBrugsen;
using ScraperLibrary.Interfaces;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Exceptions;
using ScraperLibrary.Lidl;
using ScraperLibrary.Dagrofa.Meny;

namespace DesktopApplication
{
    public class GrocerOperations
    {
        private readonly Dictionary<string, IAvisScraper> _scrapers;

        public GrocerOperations()
        {
            _scrapers = new Dictionary<string, IAvisScraper>
            {
                { "Rema", new RemaAvisScraper() },
                { "365 Discount", new _365AvisScraper() },
                { "Kvickly", new KvicklyAvisScraper() },
                { "Brugsen", new BrugsenAvisScraper() },
                { "SuperBrugsen", new SuperBrugsenAvisScraper() },
                { "Lidl", new LidlAvisScraper() },
                { "Meny", new MenyAvisScraper() }
            };
        }

        public async Task<AvisDTO?> ScrapeAvis(string scraperKey, Action<int> progressCallback, CancellationToken token, int companyId)
        {
            if (!_scrapers.TryGetValue(scraperKey, out var scraper))
                throw new ArgumentException($"Scraper not found for key: {scraperKey}");

            try
            {
                return await scraper.GetAvis(progressCallback, token, companyId);
            }
            catch (CannotReachWebsiteException e)
            {
                throw;
            }
            catch
            {
                return null; 
            }
        }
    }
}
