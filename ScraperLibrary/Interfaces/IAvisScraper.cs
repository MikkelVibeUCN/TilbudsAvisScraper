namespace ScraperLibrary.Interfaces
{
    public interface IAvisScraper
    {
        Task<string> FindAvisUrl(string url);

        string GetImageUrl(string input, int pageNumber);

        Task DownloadAllPagesAsImages(string url);
    }
}
