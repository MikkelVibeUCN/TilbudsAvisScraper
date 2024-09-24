using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Interfaces
{
    public interface IProductScraper
    {
        Task<List<Product>> GetAllProductsFromPage(string url);
    }
}
