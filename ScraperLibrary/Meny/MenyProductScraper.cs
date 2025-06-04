using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Meny
{
    public class MenyProductScraper : Scraper, IProductScraper
    {
        public Task<List<ProductDTO>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId, dynamic JSON = null)
        {





        }


        private ProductDTO ExtractProductFromProductJSON(dynamic productJSON, string avisExternalId)
        {

        }
    }
}