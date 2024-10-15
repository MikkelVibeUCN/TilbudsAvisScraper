using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Interfaces
{
    public interface IProductScraper
    {
        Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId);
    }
}
