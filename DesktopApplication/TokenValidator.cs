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
            _APIRestUserClient = new APIUserRestClient("http://94.72.103.138:8801/api/v1/");
        }

        public async Task<bool> IsTokenValidForAction(string token, int permissionRequired)
        {
            return await _APIRestUserClient.IsTokenValidForAction(token, permissionRequired);
        }
    }
}
