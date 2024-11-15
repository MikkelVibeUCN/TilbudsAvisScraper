using APIIntegrationLibrary.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplication
{
    public class TokenValidator
    {
        private readonly APIUserRestClient _APIRestUserClient;
        public TokenValidator()
        {
            _APIRestUserClient = new APIUserRestClient("https://localhost:5001/api/v1");
        }

        public async Task<bool> IsTokenValidForAction(string token, int permissionRequired)
        {
            return await _APIRestUserClient.IsTokenValidForAction(token, permissionRequired);
        }
    }
}
