using ScraperLibrary._365_Discount;
using ScraperLibrary.COOP.Brugsen;
using ScraperLibrary.COOP.Kvickly;
using ScraperLibrary.COOP.SuperBrugsen;
using ScraperLibrary.Interfaces;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Exceptions;

namespace AutomaticScraperConsoleApp
{
    public class Operations
    {
        private static readonly Dictionary<int, IAvisScraper> _scrapers = new Dictionary<int, IAvisScraper>
        {
            { 1, new RemaAvisScraper() },
            { 2, new _365AvisScraper() },
            { 3, new KvicklyAvisScraper() },
            { 4, new BrugsenAvisScraper() },
            { 5, new SuperBrugsenAvisScraper() }
        };

        public static async Task<AvisDTO> ScrapeAvis(int companyId)
        {
            if (!_scrapers.TryGetValue(companyId, out var scraper))
            {
                throw new Exception($"Scraper not found for companyId: {companyId}");
            }

            return await scraper.GetAvis(progress => Console.WriteLine($"Progress: {progress}%"), CancellationToken.None, companyId);
        }

        public static Dictionary<int, IAvisScraper> GetScrapers() => _scrapers;
    }
}