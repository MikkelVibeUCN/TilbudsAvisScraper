using TilbudsAvisLibrary;
using System.Web;
using ScraperLibrary.Interfaces;
using TilbudsAvisLibrary.Entities;
using System.Collections;
using System.Globalization;

namespace ScraperLibrary
{
    public class RemaAvisScraper : Scraper, IAvisScraper
    {
        private const string RemaImageFolder = "RemaImages";
        public RemaAvisScraper()
        {
            Directory.CreateDirectory(RemaImageFolder);
        }

        public async Task<string> FindAvisUrl(string url)
        {
            var response = await Scraper.CallUrl(url);

            string searchString = "href=\"/avis/";
            int startIndex = response.IndexOf(searchString) + searchString.Length;
            int endIndex = response.IndexOf("\"", startIndex);

            string substring = response.Substring(startIndex, endIndex - startIndex);

            return url + "/" + substring + "/1";
        }

        public async Task<Avis> GetAvis(string input)
        {
            string avisUrl = await FindAvisUrl(input);

            string searchString = "avis";
            string endSearchKey = "/";

            string externalId = GetInformationFromHtml<string>(avisUrl, searchString, endSearchKey, endSearchKey);

            List<Page> pages = await GetPagesFromUrl(avisUrl);

            return new Avis(externalId, DateTime.MinValue, DateTime.MinValue, pages);
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
            var response = await Scraper.CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            int retryCount = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                response = await Scraper.CallUrl(nextPageUrl);

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

            var response = await Scraper.CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            int retryCount = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                try
                {
                    response = await Scraper.CallUrl(nextPageUrl);
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
            string imagePath = Path.Combine(RemaImageFolder, imageName);
        
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

        public async Task<(DateTime, DateTime)> GetAvisDates(string url, string inputExternalAvisId)
        {
            IFormatProvider danishDateFormat = new CultureInfo("da-DK");
            IFormatProvider americanDateFormat = new CultureInfo("en-US");

            string startingHtml = await Scraper.CallUrl(url);
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
                    foreach (string datePart in dateParts)
                    {
                        Console.WriteLine(datePart);
                    }
                    return (ConvertStringToDate(dateParts[0], danishDateFormat, americanDateFormat), ConvertStringToDate(dateParts[1], danishDateFormat, americanDateFormat));
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