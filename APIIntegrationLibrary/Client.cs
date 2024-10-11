using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary
{
    public abstract class Client
    {
        protected HttpClient _httpClient;
        public Client()
        {
            _httpClient = new HttpClient();
        }
    }
}
