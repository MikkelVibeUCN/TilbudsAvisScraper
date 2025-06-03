using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Interfaces
{
    public interface IAvisScraper
    {
        Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId);
    }
}
