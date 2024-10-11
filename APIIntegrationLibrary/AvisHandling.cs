using Newtonsoft.Json;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APIIntegrationLibrary
{
    public class AvisHandling : Client
    {
        private RemaAvisScraper _remaAvisScraper;
        public AvisHandling()
        {
            _remaAvisScraper = new RemaAvisScraper();
        }
        
        public async Task<bool> SubmitAvis(Avis avis, string token)
        {
            var content = JsonConvert.SerializeObject(avis);

            var buffer = Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);

            HttpResponseMessage response = await _httpClient.PostAsync($"https://localhost:7133/api/v1/Avis?companyId={1}&token={token}", byteContent);

            return response.IsSuccessStatusCode;
        }
    }
}
