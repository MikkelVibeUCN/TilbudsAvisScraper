using ScraperLibrary;
using System.Text.Json;
using TilbudsAvisLibrary.Entities;
namespace ScraperConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (HttpClient client = new())
            {
                Task<List<Product>> remaProducts = Task.Run(async () =>
                {
                    return await new RemaProductScraper().GetAllProductsFromPage("https://shop.rema1000.dk/avisvarer");
                });

                Task<Avis> remaAvis = Task.Run(async () =>
                {
                    return await new RemaAvisScraper().GetAvis("https://rema1000.dk/avis");
                });

                await Task.WhenAll(remaProducts, remaAvis);

                remaAvis.Result.Products = remaProducts.Result;

                var json = JsonSerializer.Serialize(remaAvis.Result);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);

                HttpResponseMessage response = await client.PostAsync("https://localhost:5001/api/avis", byteContent);

                Console.WriteLine(response.ToString());
            }
        }
    }
}