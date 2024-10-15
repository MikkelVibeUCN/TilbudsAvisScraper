using ScraperLibrary._365_Discount;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DesktopApplication
{
    public class GrocerOperations
    {
        public async Task<Avis?> ScrapeRemaAvis(Action<int> progressCallback, CancellationToken token)
        {
            try
            {
                return await new RemaAvisScraper().GetAvis(progressCallback, token);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Avis?> Scrape365Avis(Action<int> progressCallback)
        {
            try
            {
                return await new _365AvisScraper().GetAvis(progressCallback, new CancellationToken());
            }
            catch
            {
                return null;
            }
        }
    }
}