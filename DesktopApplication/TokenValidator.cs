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
        public TokenValidator(string APIUrl)
        {
            _APIRestUserClient = new APIUserRestClient(APIUrl);
        }

        public async Task<bool> IsTokenValidForAction(string token, int permissionRequired)
        {
            return await _APIRestUserClient.IsTokenValidForAction(token, permissionRequired);
        }
    }
}
