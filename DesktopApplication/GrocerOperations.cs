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
        public async Task<Avis> ScrapeRemaAvis(Action<int> progressCallback)
        {
            return await new RemaAvisScraper().GetAvis(/*progressCallback*/);
        }

        public async Task AnotherProcessingMethod()
        {
            await Task.Delay(300);
        }
    }
}
