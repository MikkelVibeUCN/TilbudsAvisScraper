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
            _APIRestUserClient = new APIUserRestClient(APIUrl,  null);
        }
         
        public void SetAuthToken(string token)
        {
           _APIRestUserClient.SetAuthToken(token);
        }

        public async Task<bool> IsTokenValidForAction(int permissionRequired)
        {
            return await _APIRestUserClient.IsTokenValidForAction(permissionRequired);
        }
    }
}
