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
            string middleString = "";
            try
            {
                string searchString = "data-id=\"page" + pageNumber;
                int startIndex = input.IndexOf(searchString) + searchString.Length;
                int endIndex = input.IndexOf("</div>", startIndex);

                middleString = input.Substring(startIndex, endIndex - startIndex);
                //Console.WriteLine(middleString);

                searchString = "&quot;";
                startIndex = middleString.IndexOf(searchString) + searchString.Length;
                endIndex = middleString.IndexOf("&quot;);", startIndex);

                string encodedUrl = middleString.Substring(startIndex, endIndex - startIndex);

                string decodedUrl = HttpUtility.HtmlDecode(encodedUrl);

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
            var response = await IScraping.CallUrl(url);

            int lastPage = FindTotalPagesInPaper(response);

            int retryCount = 0;

            for (int i = 1; i <= lastPage; i++)
            {
                string nextPageUrl = url.Substring(0, url.Length - 1) + i;

                response = await IScraping.CallUrl(nextPageUrl);

                try
                {
                    SaveImage(nextPageUrl, GetImageUrl(response, i), i);
                    retryCount = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save image number " + i + " " + e.Message);
                    Console.WriteLine("Retry number " + retryCount+1);
                    if(retryCount < 4) { i--; retryCount++; } 
                    else { Console.WriteLine("Too many attempts, skipping"); }
                }
                
            }
        }
        private void SaveImage(string nextPageUrl, string imageUrl, int i)
        {
            string imageName = $"image{i}.jpg";
            string imagePath = Path.Combine(RemaImageFolder, imageName);

            ImageDownloader.DownloadImage(imageUrl, imagePath);

            RemaAvis.AddPage(new Page(imagePath));

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
    }
}