using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary
{
    public abstract class HClient
    {
        protected HttpClient _httpClient;
        public HClient()
        {
            _httpClient = new HttpClient();
        }
    }
}
