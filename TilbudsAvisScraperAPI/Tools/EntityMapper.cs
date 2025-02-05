using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Tools
{
    public static class EntityMapper
    {
        public static Avis MapToEntity(AvisDTO avis)
        {
            List<Product> products = ProductDTOMapper.MapToEntity(avis.Products, avis.ExternalId);

            return new Avis
            {
                Products = products,
                ValidFrom = avis.ValidFrom,
                ValidTo = avis.ValidTo,
                ExternalId = avis.ExternalId
            };
        }

        public static ProductDTO? MapCompanyToFullProductDTO(List<Company> companies)
        {
            ProductDTO? product = null;

            foreach (var company in companies)
            {
                var products = ProductDTOMapper.ConvertProductsInCompanyToDTO(company);

                if (product == null && products.Any())
                {
                    var firstProduct = products.First();
                    product = new ProductDTO
                    {
                        Amount = firstProduct.Amount,
                        Description = firstProduct.Description,
                        ExternalId = firstProduct.ExternalId,
                        Id = firstProduct.Id,
                        ImageUrl = firstProduct.ImageUrl,
                        Name = firstProduct.Name,
                        NutritionInfo = firstProduct.NutritionInfo,
                        Prices = new List<PriceDTO>()
                    };
                }

                foreach (var prod in products)
                {
                    product?.Prices.Add(prod.Prices.First());
                }
            }

            return product;
        }
    }
}
