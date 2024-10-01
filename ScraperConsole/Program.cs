using ScraperLibrary;
using TilbudsAvisLibrary.Entities;
namespace ScraperConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (HttpClient client = new())
            {
                Task remaProductsTask = Task.Run(async () =>
                {
                    IEnumerable<Product> remaProducts = await new RemaProductScraper().GetAllProductsFromPage("https://shop.rema1000.dk/avisvarer");

                    foreach (var item in remaProducts)
                    {
                        Console.WriteLine(item.ToString());
                    }
                });

                Task remaScraperTask = Task.Run(async () =>
                {
                    RemaAvisScraper remaScraper = new RemaAvisScraper();
                    string result = await remaScraper.FindAvisUrl("https://rema1000.dk/avis");

                    await remaScraper.DownloadAllPagesAsImages(result);
                });

                await Task.WhenAll(remaProductsTask, remaScraperTask);


            }



            

            



        }
    }
}