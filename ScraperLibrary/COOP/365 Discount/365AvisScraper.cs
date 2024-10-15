using PuppeteerSharp;
using ScraperLibrary.COOP;
using ScraperLibrary.COOP._365_Discount;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary._365_Discount
{
    public class _365AvisScraper : COOPAvisScraper, IAvisScraper
    {
        private string AvisUrl = "https://365discount.coop.dk/365avis/";
        private _365ProductScraper _productScraper;

        public _365AvisScraper()
        {
            _productScraper = new _365ProductScraper();
        }
        public Task DownloadAllPagesAsImages(string url)
        {
            throw new NotImplementedException();
        }
        public async Task<string> FindAvisUrl(string url)
        {
            return await GetCurrentAvisExternalId(url);
        }

        public async Task<Avis> GetAvis(Action<int> progressCallback, CancellationToken token)
        {
            string externalAvisId = await FindAvisUrl(AvisUrl);

            List<Product> products = await _productScraper.GetAllProductsFromPage(progressCallback, token, externalAvisId);

            throw new NotImplementedException();
        }

        public string GetImageUrl(string input, int pageNumber)
        {
            throw new NotImplementedException();
        }
    }
}
