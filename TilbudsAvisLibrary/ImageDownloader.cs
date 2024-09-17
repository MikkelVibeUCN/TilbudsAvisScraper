using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary
{
    public static class ImageDownloader
    {
        public static async void DownloadImage(string imageUrl, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode(); 

                using (Stream imageStream = await response.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await imageStream.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }
}
