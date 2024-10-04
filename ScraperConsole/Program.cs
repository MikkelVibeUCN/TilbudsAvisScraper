using DAL.Data.DAO;
using Newtonsoft.Json.Linq;
using ScraperLibrary;
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
                //await new AvisDAO(new ProductDAO()).Add(await new RemaAvisScraper().GetAvis(), 1, 3);

                Console.WriteLine(await new RemaProductScraper().GetProductJson(63861));
            }
        }
    }
}