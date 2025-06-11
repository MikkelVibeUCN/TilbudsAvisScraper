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
        private readonly int merchantId;
        private readonly string baseFetchUrl;
        public DagrofaProductScraper(int merchantId, string baseFetchUrl)
        {
            this.merchantId = merchantId;
            this.baseFetchUrl = baseFetchUrl;
        }
        public async Task<List<ProductDTO>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId, dynamic JSON = null)
        {
            List<ProductDTO> products = new List<ProductDTO>();
            
            JSON = await GetProductsJson();

            foreach (MenyProductDTO product in JSON)
            {
                products.Add(MapJSONToDTO(product));
            }

            return products;
        }

        private async Task<List<MenyProductDTO>> GetProductsJson(int pageSize = 10000)
        {
            string path = "/Product/query?";
            string parameters = $"merchantId={merchantId}&pageNumber=0&pageSize={pageSize}&globalAdvertisements=true";

            DTO result = await GetJSON<DTO>(baseFetchUrl + path + parameters);

            return result.products;
        }


        private ProductDTO MapMenyDTOToDTO(MenyProductDTO menyProduct)
        {
            return new ProductDTO
            {
                ExternalId = menyProduct.sku,
                Title = menyProduct.productDisplayName,
                Price = menyProduct.price,
                ImageUrl = menyProduct.mediumResImg,
                Description = menyProduct.summary,
            };
        }

        private string CreateTitle(MenyProductDTO menyProduct) 
        {

        }

        private List<PriceDTO> CreatePrices(string avisExternalId, MenyProductDTO product)
        {
            List<PriceDTO> prices = new List<PriceDTO>
            {
                new PriceDTO
                {
                    ExternalAvisId = avisExternalId,
                    CompareUnit = compareUnit,
                    Price = price,
                }
            };
            return prices;
        }



        private class MenyProductDTO
        {
            public int id { get; set; }
            public string productDisplayName { get; set; }
            public string sku { get; set; }
            public string assortmentNumber { get; set; }
            public float price { get; set; }
            public float discountPrice { get; set; }
            public float discountAmount { get; set; }
            public float discountMaxQuantity { get; set; }
            public string summary { get; set; }
            public bool advertisementProduct { get; set; }
            public int categoryId { get; set; }
            public bool isTobacco { get; set; }
            public string lowResImg { get; set; }
            public string mediumResImg { get; set; }
            public string highResImg { get; set; }
            public bool ageRestricted { get; set; }

        }
        private class DTO
        {
            public List<MenyProductDTO> products { get; set; }
            public int total { get; set; }

        }
    } 
}
