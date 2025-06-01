using TilbudsAvisLibrary.DTO;
using DAL.Data.Interfaces;
using TilbudsAvisLibrary;
using TilbudsAvisScraperAPI.Tools;
using TilbudsAvisLibrary.Entities;

namespace TilbudsAvisScraperAPI.Services
{
    public class ProductService
    {
        private readonly IProductDAO _productDAO;
        public ProductService(IProductDAO productDAO)
        {
            _productDAO = productDAO;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters)
        {
            return await _productDAO.GetProducts(parameters);
        }

        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            return await _productDAO.GetProductCountAsync(parameters);
        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSerach(ProductQueryParameters parameters)
        {
            return await _productDAO.GetValidCompanyNamesFromProductSearch(parameters);
        }

        public async Task<ProductDTO?> GetProductAsync(int id)
        {
            List <Company> companies = await _productDAO.GetProductWithInformationAsync(id);

            return companies.Count == 0 ? null : EntityMapper.MapCompanyToFullProductDTO(companies);
        }
    }
}
