using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<string> productStrings = GetProductStrings(response);

            foreach (var productString in productStrings)
            {
                Product product = CreateProduct(productString);
                products.Add(product);
            }
            return products;
        }

        public Product CreateProduct(string productContainedHtml)
        {
            string name = GetNameFromHtml(productContainedHtml);
            string description = GetDescriptionFromHtml(productContainedHtml);
            float price = GetPriceFromHtml(productContainedHtml);
            string compareUnit = GetCompareUnitFromDescription(description);
            float amount = CalculateAmount(price, compareUnit, description);

            List<Price> prices = CreatePrices();

            return new Product(prices, null, name, "", description, -1, null, amount);

            throw new NotImplementedException();
        }

        private string GetNameFromHtml(string productHtml)
        {
            throw new NotImplementedException();
        }

        private string GetDescriptionFromHtml(string productContainedHtml)
        {
            throw new NotImplementedException();
        }

        private float GetPriceFromHtml(string productContainedHtml)
        {
            throw new NotImplementedException();
        }

        private string GetCompareUnitFromDescription(string description)
        {
            throw new NotImplementedException();
        }

        private float CalculateAmount(float price, string compareUnit, string description)
        {
            throw new NotImplementedException();
        }

        private List<Price> CreatePrices()
        {
            throw new NotImplementedException();
        }

        private List<string> GetProductStrings(string html)
        {
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

                // Logic to get the first product since it has information before instead of after
                if (productStrings.Count == 0)
                {
                    int firstEndIndex = startIndex + 50;
                    startIndex = firstEndIndex - 4000;
                    productStrings.Add(remainingHtml.Substring(startIndex, firstEndIndex - startIndex));
                    remainingHtml = remainingHtml.Substring(firstEndIndex);
                    continue;
                }

                startIndex -= 50;

                int endIndex = remainingHtml.IndexOf(",-", startIndex + 55);
                if (endIndex == -1)
                {
                    endIndex = startIndex + 4000;
                }
                endIndex -= 50;

                string productString = remainingHtml.Substring(startIndex, endIndex - startIndex);
                productStrings.Add(productString);

                remainingHtml = remainingHtml.Substring(endIndex);
            }

            return productStrings;
        }
    }
}
