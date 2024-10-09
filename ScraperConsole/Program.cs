using DAL.Data.DAO;
using Newtonsoft.Json.Linq;
using ScraperLibrary.Rema;
using System.Net.Http.Headers;
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
                //Avis avis = await new RemaAvisScraper().GetAvis();

                //await new AvisDAO(new ProductDAO()).Add(avis, 1, 3);

                // Check if the JSON file exists
                //if (File.Exists("avis.json"))
                //{
                //    // Read the JSON file
                //    string json = File.ReadAllText("avis.json");
                //
                //    var options = new JsonSerializerOptions
                //    {
                //        PropertyNameCaseInsensitive = true,
                //        IncludeFields = false
                //    };
                //
                //    // Deserialize the JSON to avis object
                //    avis = JsonSerializer.Deserialize<Avis>(json, options);
                //}
                //else
                //{
                //    // Fetch the avis from the scraper
                //    avis = await new RemaAvisScraper().GetAvis();
                //
                //    string writeJson = JsonSerializer.Serialize(avis);
                //
                //    // Save JSON to file
                //    File.WriteAllText("avis.json", writeJson);
                //}
                //
                //while (true)
                //{
                //    try
                //    {
                //        await new AvisDAO(new ProductDAO()).Add(avis, 1, 3);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e);
                //    }
                //}


                var html = await RemaAvisScraper.CallUrl("https://rema1000.dk/avis");
                
                File.WriteAllText("avisWithKommende.html", html);


                //ProductDAO productDAO = new(new NutritionInfoDAO(), new PriceDAO());
                //RemaProductScraper scraper = new();
                //Console.WriteLine("get products");
                //List<Product> products = await productDAO.GetAll(3);
                //Console.WriteLine("got products");
                //Console.WriteLine("Updating products");
                //foreach (Product product in products)
                //{
                //    product.Amount = scraper.GetAmountInProduct(product.Description, product.Prices);
                //    await productDAO.Update(product, 3);
                //}
            }
        }
    }
}