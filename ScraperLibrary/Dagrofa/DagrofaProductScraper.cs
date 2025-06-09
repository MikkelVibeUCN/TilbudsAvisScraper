using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Dagrofa
{
    public class DagrofaProductScraper : Scraper, IProductScraper
    {

        public DagrofaProductScraper(int merchantId)
        {
            
        }
        public Task<List<ProductDTO>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId, dynamic JSON = null)
        {




            throw new NotImplementedException();

        }

        public async Task<dynamic> GetProductsJson(int merchantId, int pageSize = 10000)
        {
            string baseUrl = "https://longjohnapifrontdoor-hndubyhwdyb6bzbj.z01.azurefd.net";
            string path = "/Product/query?";
            string parameters = $"{merchantId}&pageNumber=0&pageSize=${pageSize}&displayedInStore=true";

            return await client.GetAsync(baseUrl + path + parameters);
        }
    }
}
