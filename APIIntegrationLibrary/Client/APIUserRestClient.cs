using APIIntegrationLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary.Client
{
    public class APIUserRestClient : BaseClient<APIUserDTO>
    {
        public APIUserRestClient(string baseUrl) : base(baseUrl, "APIUser")
        {
        }



        public async Task<bool> IsTokenValidForAction(string token, int permissionRequired)
        {
            var response = await _restClient.RequestAsync(RestSharp.Method.Get, $"{_defaultEndPoint}?token={token}&permissionLevel={permissionRequired}");

            return response.IsSuccessful ? true : false;
        }
    }
}
