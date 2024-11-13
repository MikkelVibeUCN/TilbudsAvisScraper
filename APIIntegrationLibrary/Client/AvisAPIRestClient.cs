using APIIntegrationLibrary.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace APIIntegrationLibrary.Client
{
    public class AvisAPIRestClient : BaseClient<AvisDTO>
    {
        private HttpClient _httpClient { get; set; }

        public AvisAPIRestClient(string uri, HttpClient httpClient) : base(uri, "Avis")
        {
            _httpClient = httpClient;
        }
        public async Task<bool> SubmitAvis(Avis avis, int companyId, string token)
        {
            var content = JsonConvert.SerializeObject(avis);

            var byteContent = new StringContent(content, Encoding.UTF8, "application/json");

            string postUrl = $"https://localhost:5001/api/v1/Avis?companyId={companyId}&token={token}";

            HttpResponseMessage response = await _httpClient.PostAsync(postUrl, byteContent);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception("Error: " + response.StatusCode);
            }
        }
        public async Task<int> CreateAsync(AvisDTO avis, int companyId, string token)
        {
            return await CreateAsync(avis, $"{_defaultEndPoint}?companyId={companyId}&token={token}");
        }
    }
}
