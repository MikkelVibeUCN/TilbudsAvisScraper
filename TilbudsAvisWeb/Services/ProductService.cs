using APIIntegrationLibrary.DTO;
using APIIntegrationLibrary.Interfaces;
using TilbudsAvisLibrary;

namespace TilbudsAvisWeb.Services
{
    public class ProductService
    {
        private readonly IProductAPIRestClient _productAPIRestClient;
        public ProductService(IProductAPIRestClient productAPIRestClient)
        {
            _productAPIRestClient = productAPIRestClient;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters)
        {
            return await _productAPIRestClient.GetProductsAsync(parameters);
        }

        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            return await _productAPIRestClient.GetProductCountAsync(parameters);
        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSerach(ProductQueryParameters parameters)
        {
            return await _productAPIRestClient.GetValidCompanyNamesFromProductSearch(parameters);
        }

        public async Task<ProductDTO?> GetProductAsync(int id)
        {
            return await _productAPIRestClient.GetAsync(id);
        }
    }
}
