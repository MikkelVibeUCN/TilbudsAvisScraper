using TilbudsAvisLibrary;
using System.Web;
using ScraperLibrary.Interfaces;
using TilbudsAvisLibrary.Entities;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Rema
{
    public class RemaAvisScraper : Scraper, IAvisScraper
    {
        private const string _remaAvisPageUrl = "https://rema1000.dk/avis";
        private const string _remaImageFolder = "RemaImages";
        private RemaProductScraper _productScraper = new RemaProductScraper();
        public RemaAvisScraper()
        {
            
        }

        public async Task<string> FindExternalAvisId(string url)
        {
            var response = await CallUrl(url);

            File.WriteAllText("resultNoKommende.html", response);


            string rarePatternAssosiatedWithComingAvis = "Kommende";
            string avisPattern = "a href=\"/avis/";

            if (response.Contains(rarePatternAssosiatedWithComingAvis))
            {
                int indexOfKommendeAvis = response.IndexOf(avisPattern);

                response = response.Substring(indexOfKommendeAvis + 50);
            }
            string externalId = GetInformationFromHtml<string>(response, avisPattern, avisPattern, "\"");

            return externalId;
        }

        public async Task<string> FindAvisUrl(string url)
        {
            return url + "/" + await FindExternalAvisId(url) + "/1";
        }

        public async Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            string avisUrl = await FindAvisUrl(_remaAvisPageUrl);
            progressCallback(4);

            string externalId = await FindExternalAvisId(avisUrl);
            progressCallback(6);

            var getDatesTask = GetAvisDates("https://rema1000.dk/avis", externalId);
            progressCallback(12);
            //var getPagesTask = Task.Run(() => GetPagesFromUrl(avisUrl));
            var getProductsTask = await _productScraper.GetAllProductsFromPage(progressCallback, token, externalId, companyId);
            progressCallback(100);

            return RemoveDuplicateProductsFromAvis(new AvisDTO
            {
                ExternalId = externalId,
                ValidFrom = getDatesTask.Result.Item1,
                ValidTo = getDatesTask.Result.Item2,
                Products = getProductsTask
            });
        }

        public string GetImageUrl(string input, int pageNumber)
        {
            try
            {
                string searchString = "data-id=\"page" + pageNumber;
                string startSearchKey = "&quot;";
                string endSearchKey = "&quot;);";

                string encodedUrl = GetInformationFromHtml<string>(input, searchString, startSearchKey, endSearchKey);

                return HttpUtility.HtmlDecode(encodedUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                File.WriteAllText($"output{input[0]}.html", input);
                throw new Exception($"GetImageUrl failed. log saved to {input[0]}");
            }
        }

        public async Task DownloadAllPagesAsImages(string url)
        {
            var response = await CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            int retryCount = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                response = await CallUrl(nextPageUrl);

                try
                {
                    SaveImage(nextPageUrl, GetImageUrl(response, i), i);
                    retryCount = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save image number " + i + " " + e.Message);
                    Console.WriteLine("Retry number " + retryCount + 1);
                    if (retryCount < 4) { i--; retryCount++; }
                    else { Console.WriteLine("Too many attempts, skipping"); }
                }
            }
        }

        public async Task<List<Page>> GetPagesFromUrl(string url)
        {
            List<Page> resultingPages = new List<Page>();

            var response = await CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            int retryCount = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                try
                {
                    response = await CallUrl(nextPageUrl);
                    resultingPages.Add(new Page(GetImageUrl(response, i), i));
                    Console.WriteLine("Added page " + i);
                    retryCount = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to add image number " + i + " " + e.Message);
                    Console.WriteLine("Retry number " + retryCount + 1);
                    if (retryCount < 4) { i--; retryCount++; }
                    else { Console.WriteLine("Too many attempts, skipping"); }
                }
            }
            return resultingPages;
        }
        private void SaveImage(string nextPageUrl, string imageUrl, int i)
        {
            string imageName = $"image{i}.jpg";
            string imagePath = Path.Combine(_remaImageFolder, imageName);

            ImageDownloader.DownloadImage(imageUrl, imagePath);

            Console.WriteLine("Successfully saved page " + i);
        }
        private int FindTotalPagesInPaper(string input)
        {
            string searchString = "sgn-pp__progress-label\">";
            int startIndex = input.IndexOf(searchString) + searchString.Length;
            int endIndex = input.IndexOf("<", startIndex);

            string pageNumberString = input.Substring(startIndex, endIndex - startIndex);

            string[] parts = pageNumberString.Split('/');
            string lastPageString = parts[1].Trim();

            int lastPage = int.Parse(lastPageString);

            return lastPage;
        }

        private async Task<(DateTime, DateTime)> GetAvisDates(string url, string inputExternalAvisId)
        {
            IFormatProvider danishDateFormat = new CultureInfo("da-DK");
            IFormatProvider americanDateFormat = new CultureInfo("en-US");

            string startingHtml = await CallUrl(url);
            string cutDownHtml = startingHtml;

            string searchString = "<a href=\"/avis/";
            string endSearchKey = "\"";
            string searchKey = searchString;

            int currentIndex = 0;
            while (currentIndex != url.Length)
            {
                string avisExternalId = GetInformationFromHtml<string>(cutDownHtml, searchString, searchKey, endSearchKey);

                if (avisExternalId.Equals(inputExternalAvisId))
                {
                    searchString = "<div class=\"flex flex-col gap-0 text-center\">";
                    searchKey = "base\">";
                    endSearchKey = "</h4>";

                    string dates = GetInformationFromHtml<string>(cutDownHtml, searchString, searchKey, endSearchKey);

                    string[] dateParts = dates.Split(" - ");

                    DateTime startDate = ConvertStringToDate(dateParts[0], danishDateFormat, americanDateFormat);
                    DateTime endDate = ConvertStringToDate(dateParts[1], danishDateFormat, americanDateFormat);

                    startDate = startDate.AddSeconds(1);
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                    return (startDate, endDate);
                }
                else
                {
                    int cutPoint = cutDownHtml.IndexOf(searchString);

                    string tempString = cutDownHtml.Substring(cutPoint);

                    int endIndex = tempString.IndexOf("</a>", currentIndex);

                    cutDownHtml = cutDownHtml.Substring(cutPoint + endIndex);
                }
            }
            throw new Exception("Avis not found");
        }
    }
}