using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Interfaces
{
    public interface IAvisScraper
    {
        

        Task<string> FindAvisUrl(string url);

        string GetImageUrl(string input, int pageNumber);

        Task DownloadAllPagesAsImages(string url);
    }
}
