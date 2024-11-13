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


        
    }
}
