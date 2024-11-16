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
            var queryString = GetQueryString(parameters);

            var response = await _restClient.RequestAsync<IEnumerable<ProductDTO>>(Method.Get, $"products{queryString}");

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new Exception($"Error retrieving Products. Message was {response.Content}");
            }

            return response.Data ?? Enumerable.Empty<ProductDTO>();
        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSearch(ProductQueryParameters parameters)
        {
            return await GetAllAsync<string>("products/retailers", parameters);
        }

        private string GetQueryString(ProductQueryParameters parameters)
        {
            var queryString = "?";

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                queryString += $"SortBy={parameters.SortBy}&";
            }

            if (parameters.PageNumber > 0)
            {
                queryString += $"PageNumber={parameters.PageNumber}&";
            }

            if (parameters.PageSize > 0)
            {
                queryString += $"PageSize={parameters.PageSize}&";
            }

            if (!string.IsNullOrEmpty(parameters.Retailer))
            {
                queryString += $"Retailer={parameters.Retailer}&";
            }

            // Remove the trailing '&' if there are any query parameters added
            if (queryString.EndsWith("&"))
            {
                queryString = queryString.Remove(queryString.Length - 1);
            }

            return queryString;
        }

    }
}