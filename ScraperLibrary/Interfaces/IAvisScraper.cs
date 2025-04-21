using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Interfaces
{
    public interface IAvisScraper
    {
        string GetImageUrl(string input, int pageNumber);

        Task DownloadAllPagesAsImages(string url);

        Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId);
    }
}
