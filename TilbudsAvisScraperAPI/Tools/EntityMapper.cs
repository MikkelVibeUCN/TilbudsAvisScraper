using APIIntegrationLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Tools
{
    public static class EntityMapper
    {
        public static IEnumerable<ProductDTO> MapToDTO(IEnumerable<Company> companies)
        {
            List<ProductDTO> products = new List<ProductDTO>();

            foreach (Company company in companies)
            {
                products.AddRange(ProductDTOMapper.ConvertProductsInCompanyToDTO(company));
            }
            return products;
        }

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
    }
}
