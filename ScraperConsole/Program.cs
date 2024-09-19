using PuppeteerSharp;
using System.Web;
using TilbudsAvisLibrary;
namespace ScraperConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            RemaScraper remaScraper = new RemaScraper();
            string result = await remaScraper.FindAvisUrl("https://rema1000.dk/avis");

            //await remaScraper.DownloadAllPagesAsImages(result);

            //var outResult = await IScraping.CallUrl("https://rema1000.dk/avis/ebduaEkY/2");


            //Console.WriteLine(outResult);

            //File.WriteAllText("response.html", outResult);
            //Console.WriteLine(remaScraper.GetImageUrl(outResult));

            //Console.WriteLine(remaScraper.GetImageUrl(outResult));

            ImageProcessing process = new ImageProcessing();
            process.ProcessAllImagesInFolder("RemaImages");
        }
    }
}