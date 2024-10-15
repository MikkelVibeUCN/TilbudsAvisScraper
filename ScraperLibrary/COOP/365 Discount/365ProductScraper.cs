using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP._365_Discount
{
    public class _365ProductScraper : Scraper, IProductScraper
    {
        private string ProductsLocationUrl = "https://365discount.coop.dk/365avis/";
        public async Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId)
        {
            var response = await CallUrl(ProductsLocationUrl, 5000);
            List<Product> products = new List<Product>();
            List<string> productStrings = await GetProductStrings(response);

            foreach (var productString in productStrings)
            {
                Product product = await CreateProduct(productString);
                products.Add(product);
            }
            return products;
        }

        public async Task<Product> CreateProduct(string productContainedHtml)
        {
            throw new NotImplementedException();
        }

        private async Task<List<string>> GetProductStrings(string html)
        {
            // Implement next

            string remainingHtml = html.Substring(html.IndexOf("<header id=\"coop-nav"));
            List<string> productStrings = new List<string>();

            bool reachedEnd = false;
            while (!reachedEnd)
            {
                // Split all the html into parts that all have one product each by:

                // First find where the products are by using ",-" as a searchpattern
                // Treverse in the html both up and down until another ",-" is found or if no more ",-" are found set the end index/start index 4000 after/before the last ",-" depending on the direction
                // When the ,- is found set 50 characters before it as the end index and the start index of the next product

                int startIndex = remainingHtml.IndexOf(",-");
                if (startIndex == -1)
                {
                    reachedEnd = true;
                    break;
                }

                int endIndex = remainingHtml.IndexOf(",-", startIndex + 1);
                if (endIndex == -1)
                {
                    endIndex = startIndex + 4000;
                }

                int productStartIndex = startIndex - 50;
                if (productStartIndex < 0)
                {
                    productStartIndex = 0;
                }

                string productString = remainingHtml.Substring(productStartIndex, endIndex - productStartIndex);
                productStrings.Add(productString);

                remainingHtml = remainingHtml.Substring(endIndex);
            }

            return productStrings;
        }
    }
}
