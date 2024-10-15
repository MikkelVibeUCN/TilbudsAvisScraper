using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary._365_Discount
{
    public class _365AvisScraper : Scraper, IAvisScraper
    {
        private string AvisUrl = "https://365discount.coop.dk/365avis/";
        public Task DownloadAllPagesAsImages(string url)
        {
            throw new NotImplementedException();
        }
        public async Task<string> FindAvisUrl(string url)
        {
            var response = await CallUrl(url, 5000);

            File.WriteAllText("test.html", response);

            return "";
        }

        public async Task<Avis> GetAvis(Action<int> progressCallback, CancellationToken token)
        {

            await FindAvisUrl(AvisUrl);
            throw new NotImplementedException();
        }

        public string GetImageUrl(string input, int pageNumber)
        {
            throw new NotImplementedException();
        }
    }
}
