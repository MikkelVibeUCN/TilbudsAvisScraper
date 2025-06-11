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

namespace ScraperLibrary.Dagrofa.Meny
{
    public class MenyAvisScraper : Scraper, IAvisScraper
    {
        private const string menyAvisUrl = "https://ugensavis.meny.dk/";

        private readonly IProductScraper _dagrafaProductScraper;
        public MenyAvisScraper()
        {
            _dagrafaProductScraper = new MenyProductScraper();
        }
        public async Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            List<string> urls = await GetAllChunkUrls();

            // Get the AvisID from the first chunk URL
            string avisId = GetAvisIdFromChunk(urls[0]);

            // Get dates for avis
            var response = await CallUrl(menyAvisUrl);
            var dates = IAvisScraper.ExtractAvisPeriodFromHTML(response);

            List<ProductDTO> products = await _dagrafaProductScraper.GetAllProductsFromPage(
                progressCallback,
                token,
                avisId,
                companyId
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
            EnrichmentsDTO enrichments = await EvaluateJsPropertyAsync<EnrichmentsDTO>(
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
