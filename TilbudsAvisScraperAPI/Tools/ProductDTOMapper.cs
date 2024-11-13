using APIIntegrationLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Tools
{
    public static class ProductDTOMapper
    {
        public static List<ProductDTO> ConvertProductsInCompanyToDTO(Company company)
        {
            List<ProductDTO> products = new List<ProductDTO>();

            foreach (var avis in company.Aviser)
            {
                products.AddRange(ConvertAvisProductsToDTO(avis, company.Name));
            }
            return products;
        }

        private static List<ProductDTO> ConvertAvisProductsToDTO(Avis avis, string companyName)
        {
            List<ProductDTO> products = new List<ProductDTO>();

            foreach (var product in avis.Products)
            {
                products.Add(MapProductToDTO(product, companyName, avis.ValidFrom, avis.ValidTo));
            }

            return products;
        }

        private static ProductDTO MapProductToDTO(Product product, string companyName, DateTime validFrom, DateTime validTo)
        {
            List<PriceDTO> prices = new List<PriceDTO>();
            foreach (var price in product.Prices)
            {
                prices.Add(new PriceDTO
                {
                    Price = price.PriceValue,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    CompareUnit = price.CompareUnitString,
                    CompanyName = companyName

                });
            }

            return new ProductDTO
            {
                Description = product.Description,
                Id = (int)product.Id,
                ImageUrl = product.ImageUrl,
                Name = product.Name,
                NutritionInfo = product.NutritionInfo,
                Prices = prices
            };
        }
    }
}
