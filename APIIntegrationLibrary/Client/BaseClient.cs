using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary;

namespace APIIntegrationLibrary.Client
{
    public abstract class BaseClient<T>
    {
        protected RestClient _restClient;
        protected string _defaultEndPoint;
        public BaseClient(string uri, string defaultEndpoint)
        {
            _restClient = new RestClient(new Uri(uri));
            _defaultEndPoint = defaultEndpoint;
        }
        public async Task<int> CreateAsync(T entity, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var response = await _restClient.RequestAsync<int>(Method.Post, endpoint, entity);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error creating {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }


        public async Task<T> EditAsync(T entity, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var response = await _restClient.RequestAsync<T>(Method.Put, endpoint, entity);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error editing {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<bool> DeleteAsync(int id, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var response = await _restClient.RequestAsync<bool>(Method.Delete, $"{endpoint}/{id}");

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error deleting {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var response = await _restClient.RequestAsync<IEnumerable<T>>(Method.Get, endpoint);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new Exception($"Error retrieving all {typeof(T).Name}. Message was {response.Content}");
            }

            return response.Data ?? Enumerable.Empty<T>();
        }

        public async Task<T?> GetAsync(int? id = null, string ? endpoint = null)
        {
            if(endpoint == null && id == null) { throw new Exception("Either endpoint or id must be provided"); }

            endpoint ??= $"{_defaultEndPoint}/{id}";
            var response = await _restClient.RequestAsync<T>(Method.Get, endpoint);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error retrieving {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<IEnumerable<U>> GetAllAsync<U>(string endpoint, ProductQueryParameters parameters)
        {
            var response = await _restClient.RequestAsync<IEnumerable<U>>(Method.Get, endpoint, parameters);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error retrieving all {typeof(U).Name}. Message was {response.Content}");
            }
            return response.Data;
        }
    }
}
