using APIIntegrationLibrary.DTO;
using APIIntegrationLibrary.Interfaces;
using TilbudsAvisLibrary;
using RestSharp;

namespace APIIntegrationLibrary.Client
{
    public class ProductAPIRestClient : BaseClient<ProductDTO>, IProductAPIRestClient
    {
        public ProductAPIRestClient(string uri) : base(uri, "Products")
        {
        }

        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            var response = await _restClient.RequestAsync<int>(Method.Get, "products/count", parameters);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error retrieving ProductDTO. Message was {response.Content}");
            }
            return response.Data;

        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters)
        {
            var response = await _restClient.RequestAsync<IEnumerable<ProductDTO>>(Method.Get, "products", parameters);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new Exception($"Error retrieving Products. Message was {response.Content}");
            }

            return response.Data ?? Enumerable.Empty<ProductDTO>();
        }

        public Task<IEnumerable<string>> GetValidCompanyNamesFromProductSearch(ProductQueryParameters parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSerach(ProductQueryParameters parameters)
        {
            return await GetAllAsync<string>("products/companies", parameters);
        }
    }
}