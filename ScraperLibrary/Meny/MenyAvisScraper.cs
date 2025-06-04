using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScraperLibrary.Exceptions;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Meny
{
    public class MenyAvisScraper : Scraper, IAvisScraper
    {
        const string menyAvisUrl = "https://ugensavis.meny.dk/";
        private readonly MenyProductScraper _menyProductScraper;

        public async Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            List<string> urls = await GetAllChunkUrls();
            var json = await GetFormattedJsonForAllChunks(urls);

            // Remove non-product items from the JSON
            json = RemoveNonProductsFromJSON(json);

            // Get the AvisID from the first chunk URL
            string avisId = GetAvisIdFromChunk(urls[0]);

            // Get dates for avis
            var response = await CallUrl(menyAvisUrl);
            var dates = IAvisScraper.ExtractAvisPeriodFromHTML(response);

            List<ProductDTO> products = await _menyProductScraper.GetAllProductsFromPage(
                progressCallback,
                token,
                avisId,
                companyId,
                json
            );

            return RemoveDuplicateProductsFromAvis(new AvisDTO
            {
                ExternalId = avisId,
                ValidTo = dates.ValidTo,
                ValidFrom = dates.ValidFrom,
                Products = products,
            });



            
        }


        private async Task<List<string>> GetAllChunkUrls()
        {
            EnrichmentsDTO enrichments = await Scraper.EvaluateJsPropertyAsync<EnrichmentsDTO>(
                menyAvisUrl,
                "window.staticSettings.enrichments",
                "window.staticSettings && window.staticSettings.enrichments"
            );

            return enrichments.ChunkUrls?.Values.ToList() ?? throw new Exception("Failed to get chunk urls");
        }

        public class EnrichmentsDTO
        {
            [JsonProperty("pageChunksIndexes")]
            public Dictionary<string, string> PageChunksIndexes { get; set; }

            [JsonProperty("chunkUrls")]
            public Dictionary<string, string> ChunkUrls { get; set; }
        }


        private async Task<dynamic> GetFormattedJsonForChunkUrl(string chunkUrl)
        {
            HttpResponseMessage response = await client.GetAsync(chunkUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve chunk URL: {chunkUrl}, Status Code: {response.StatusCode}");
            }
            string json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Failed to retrieve JSON from chunk URL");
            }

            // Parse the JSON and return it as a dynamic object
            return JObject.Parse(json);
        }


        private async Task<JObject> GetFormattedJsonForAllChunks(List<string> chunkUrls)
        {
            var fetchTasks = chunkUrls.Select(GetFormattedJsonForChunkUrl).ToList();
            var chunks = await Task.WhenAll(fetchTasks);

            var combinedJson = new JObject();

            foreach (var chunk in chunks)
            {
                combinedJson.Merge(chunk, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Concat,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                });
            }

            return combinedJson;
        }

        private dynamic RemoveNonProductsFromJSON(dynamic json)
        {
            if (json.enrichments == null || !(json.enrichments is JArray))
                return json;

            var filtered = new JArray();

            foreach (var item in json.enrichments)
            {
                if (item["price"] != null && item["price"].Type != JTokenType.Null)
                {
                    filtered.Add(item);
                }
            }

            json.enrichments = filtered;
            return json;
        }

        private string GetAvisIdFromChunk(string chunkUrl)
        {

            string[] parts = chunkUrl.Split(new[] { "/Papers/" }, StringSplitOptions.None);

            if (parts.Length > 1)
            {
                string afterPapers = parts[1];
                return afterPapers.Split('/')[0];
            }
            throw new Exception("Failed to get AvisID");
        }
    }
}
