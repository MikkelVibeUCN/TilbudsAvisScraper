using PuppeteerSharp;
using TilbudsAvisLibrary;
namespace ScraperConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string result = await Scraper.FindAvisUrl("https://rema1000.dk/avis");

            var outResult = await Scraper.CallUrl(result);

            //Console.WriteLine(outResult);

            File.WriteAllText("response.html", outResult);

            Console.WriteLine(result);

            Console.WriteLine(GetImageUrl(outResult));
        }

        private static string GetImageUrl(string input)
        {
            string searchString = "background-image: url(\"";
            int startIndex = input.IndexOf(searchString) + searchString.Length;
            int endIndex = input.IndexOf("\"", startIndex);

            return input.Substring(startIndex, endIndex - startIndex);
        }
    }
}