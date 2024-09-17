using TilbudsAvisLibrary;
namespace ScraperConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string result = await Scraper.FindAvisUrl("https://rema1000.dk/avis");

            Console.ReadLine();

            var outResult = await Scraper.CallUrl(result);

            //Console.WriteLine(outResult);

            // Save outResult to a txt file
            File.WriteAllText("response.txt", outResult);

            //Console.WriteLine(result);
        }
    }
}