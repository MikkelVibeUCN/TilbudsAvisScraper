using TilbudsAvisLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace APIIntegrationLibrary.Client
{
    public class APIUserRestClient : BaseClient<APIUserDTO>
    {
        public APIUserRestClient(string baseUrl, string? token) : base(baseUrl, "APIUser", token)
        {
        }

        public async Task<bool> IsTokenValidForAction(int permissionRequired)
        {
            var response = await _restClient.ExecuteAsync(CreateRequest($"{_defaultEndPoint}?permissionLevel={permissionRequired}", RestSharp.Method.Get));

            return response.IsSuccessful ? true : false;
        }
    }
}
