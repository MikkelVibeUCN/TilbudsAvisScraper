using Newtonsoft.Json;
using ScraperLibrary.Interfaces;
using System.ComponentModel.DataAnnotations;
using TilbudsAvisLibrary.DTO;


namespace ScraperLibrary.Lidl
{
    public class LidlAvisScraper : Scraper, IAvisScraper
    {
        const string _lidlAvisUrl = "https://www.lidl.dk/c/tilbudsavis/s10013730";
        const string _endPointUrl = "https://endpoints.leaflets.schwarz/v4/flyer?flyer_identifier=(FLYER ID HERE)&region_id=0&region_code=0";

        private readonly LidlProductScraper _lidlProductScraper = new LidlProductScraper();

        public async Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            string avisId = await GetAvisId();

            // Get the json
            string url = _endPointUrl.Replace("(FLYER ID HERE)", avisId);

            dynamic response = await GetJSON<dynamic>(url);

            // Set the dates for the paper (offerStartDate and offerEndDate)
            DateTime offerStart = response.flyer.offerStartDate;
            DateTime offerEnd = response.flyer.offerEndDate;

            // Get the products
            List<ProductDTO> products = await _lidlProductScraper.GetAllProductsFromPage(progressCallback, token, avisId, companyId, response.flyer.products);

            return RemoveDuplicateProductsFromAvis(new AvisDTO
            {
                ExternalId = avisId,
                ValidFrom = offerStart,
                ValidTo = offerEnd,
                Products = products
            });
        }

        public async Task<string> GetAvisId()
        {
            // Go to this url https://www.lidl.dk/c/tilbudsavis/s10013730

            var response = await CallUrl(_lidlAvisUrl);

            // Look for <h4 class="flyer__title">Tilbudsavis</h4>
            string titleTag = "<h4 class=\"flyer__title\">";
            string endTag = "</h4>";
            int searchStart = 0;

            while (true)
            {
                int titleStart = response.IndexOf(titleTag, searchStart);
                if (titleStart == -1) break;

                int titleContentStart = titleStart + titleTag.Length;
                int titleEnd = response.IndexOf(endTag, titleContentStart);
                if (titleEnd == -1) break;

                string extractedTitle = response.Substring(titleContentStart, titleEnd - titleContentStart).Trim();

                if (extractedTitle.Contains("Tilbudsavis", StringComparison.OrdinalIgnoreCase))
                {
                    // Find the start of the <a> tag before the <h4>
                    int flyerStart = response.LastIndexOf("<a ", titleStart);
                    int flyerEnd = response.IndexOf(">", flyerStart); // End of the <a> tag opening

                    if (flyerStart != -1 && flyerEnd != -1)
                    {
                        string flyerTag = response.Substring(flyerStart, flyerEnd - flyerStart);

                        // Use your method to extract the track ID from just the <a> tag
                        return GetInformationFromHtml<string>(
                            flyerTag,
                            "data-track-id=\"",
                            "data-track-id=\"",
                            "\""
                        );
                    }
                }

                searchStart = titleEnd + endTag.Length;
            }

            throw new Exception("No valid Tilbudsavis found on the page.");
        }
    }
}
