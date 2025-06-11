using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperLibrary.Dagrofa.Meny
{
    public class MenyProductScraper : DagrofaProductScraper
    {
        private const int merchantId = 558155; // Meny Merchant ID
        private const string baseFetchUrl = "https://longjohnapi-meny.azurewebsites.net";
        public MenyProductScraper() : base(merchantId, baseFetchUrl)
        {
        }
    }
}
