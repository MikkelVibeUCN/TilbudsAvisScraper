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
            // Serialize the Avis object to JSON
            var content = JsonConvert.SerializeObject(avis);

            // Use StringContent and specify the content type as application/json
            var byteContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await _httpClient.PostAsync($"https://localhost:5001/api/v1/Avis?companyId={1}&token={token}", byteContent);

            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception("Error: " + response.StatusCode);
            }
        }

    }
}
