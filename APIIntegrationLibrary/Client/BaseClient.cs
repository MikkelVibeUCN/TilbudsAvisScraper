using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TilbudsAvisLibrary;
using RestSharp;

namespace APIIntegrationLibrary.Client
{
    public abstract class BaseClient<T>
    {
        protected RestClient _restClient;
        protected string _defaultEndPoint;
        private string? _authToken;

        public BaseClient(string uri, string defaultEndpoint, string? authToken = null)
        {
            _restClient = new RestClient(new Uri(uri));
            _defaultEndPoint = defaultEndpoint;
            _authToken = authToken;
        }

        public void SetAuthToken(string? token)
        {
            _authToken = token;
        }

        public RestRequest CreateRequest(string endpoint, Method method, object? body = null)
        {
            var request = new RestRequest(endpoint, method);

            if (!string.IsNullOrEmpty(_authToken))
            {
                request.AddHeader("Authorization", $"Bearer {_authToken}");
            }

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            return request;
        }

        public async Task<int> CreateAsync(T entity, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var request = CreateRequest(endpoint, Method.Post, entity);
            var response = await _restClient.ExecuteAsync<int>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error creating {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<T> EditAsync(T entity, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var request = CreateRequest(endpoint, Method.Put, entity);
            var response = await _restClient.ExecuteAsync<T>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error editing {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<bool> DeleteAsync(int id, string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var request = CreateRequest($"{endpoint}/{id}", Method.Delete);
            var response = await _restClient.ExecuteAsync<bool>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error deleting {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string? endpoint = null)
        {
            endpoint ??= _defaultEndPoint;
            var request = CreateRequest(endpoint, Method.Get);
            var response = await _restClient.ExecuteAsync<IEnumerable<T>>(request);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new Exception($"Error retrieving all {typeof(T).Name}. Message was {response.Content}");
            }

            return response.Data ?? Enumerable.Empty<T>();
        }

        public async Task<T?> GetAsync(int? id = null, string? endpoint = null)
        {
            if (endpoint == null && id == null) { throw new Exception("Either endpoint or id must be provided"); }

            endpoint ??= $"{_defaultEndPoint}/{id}";
            var request = CreateRequest(endpoint, Method.Get);
            var response = await _restClient.ExecuteAsync<T>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error retrieving {typeof(T).Name}. Message was {response.Content}");
            }
            return response.Data;
        }

        public async Task<IEnumerable<U>> GetAllAsync<U>(string endpoint, ProductQueryParameters parameters)
        {
            var request = CreateRequest(endpoint, Method.Get, parameters);
            var response = await _restClient.ExecuteAsync<IEnumerable<U>>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error retrieving all {typeof(U).Name}. Message was {response.Content}");
            }
            return response.Data;
        }
    }
}
