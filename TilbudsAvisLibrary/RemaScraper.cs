using PuppeteerSharp;
using System.Web;

namespace TilbudsAvisLibrary
{
    public class RemaScraper : IScraping
    {
        private const string RemaImageFolder = "RemaImages";
        private Avis RemaAvis = new Avis("Rema 1000", null, null);
        public RemaScraper()
        {
            Directory.CreateDirectory(RemaImageFolder);
        }

        public async Task<string> FindAvisUrl(string url)
        {
            var response = await IScraping.CallUrl(url);

            string searchString = "href=\"/avis/";
            int startIndex = response.IndexOf(searchString) + searchString.Length;
            int endIndex = response.IndexOf("\"", startIndex);

            string substring = response.Substring(startIndex, endIndex - startIndex);

            return url + "/" + substring + "/1";
        }

        public string GetImageUrl(string input, int pageNumber)
        {
            string searchString = "data-id=\"page" + pageNumber;
            int startIndex = input.IndexOf(searchString) + searchString.Length;
            int endIndex = input.IndexOf("</div>", startIndex);

            string middleString = input.Substring(startIndex, endIndex - startIndex);
            //Console.WriteLine(middleString);

            searchString = "&quot;";
            startIndex = middleString.IndexOf(searchString) + searchString.Length;
            endIndex = middleString.IndexOf("&quot;);", startIndex);

            string encodedUrl = middleString.Substring(startIndex, endIndex - startIndex);

            string decodedUrl = HttpUtility.HtmlDecode(encodedUrl);

            return HttpUtility.HtmlDecode(encodedUrl);
        }

        public async Task DownloadAllPagesAsImages(string url)
        {
            var response = await IScraping.CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                response = await IScraping.CallUrl(nextPageUrl);
                
                SaveImage(nextPageUrl, GetImageUrl(response, i), i);
            }
        }
        private void SaveImage(string nextPageUrl, string imageUrl, int i)
        {
            string imageName = $"image{i}.jpg";
            string imagePath = Path.Combine(RemaImageFolder, imageName);

            ImageDownloader.DownloadImage(imageUrl, imagePath);

            RemaAvis.AddPage(new Page(imagePath));

            Console.WriteLine(nextPageUrl);
            Console.WriteLine(imageUrl);
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
    }
}