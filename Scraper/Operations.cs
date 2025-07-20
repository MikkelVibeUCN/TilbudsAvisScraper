using ScraperLibrary._365_Discount;
using ScraperLibrary.COOP.Brugsen;
using ScraperLibrary.COOP.Kvickly;
using ScraperLibrary.COOP.SuperBrugsen;
using ScraperLibrary.Interfaces;
using ScraperLibrary.Lidl;
using ScraperLibrary.Rema;
using TilbudsAvisLibrary.DTO;

namespace Scraper
{
    public class Operations
    {
        private static readonly Dictionary<int, IAvisScraper> _scrapers = new Dictionary<int, IAvisScraper>
        {
            { 1, new RemaAvisScraper() },
            { 2, new _365AvisScraper() },
            { 3, new KvicklyAvisScraper() },
            { 4, new BrugsenAvisScraper() },
            { 5, new SuperBrugsenAvisScraper() },
            { 7, new LidlAvisScraper() }
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