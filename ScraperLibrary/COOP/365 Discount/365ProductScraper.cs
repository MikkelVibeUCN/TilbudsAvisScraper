using ScraperLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

            List<Price> prices = CreatePrices(productContainedHtml, compareUnit);

            float amount = CalculateAmount(price, compareUnit, description);

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
            return GetInformationFromHtml<float>(productContainedHtml, ",-", ">", ",", -10);
        }

        private string GetCompareUnitFromDescription(string description)
        {
            throw new NotImplementedException();
        }

        private float CalculateAmount(float price, string compareUnit, string description)
        {
            throw new NotImplementedException();
        }

        private List<Price> CreatePrices(string productContainedHtml, string compareUnit)
        {
            return new List<Price> { new Price(GetPriceFromHtml(productContainedHtml), compareUnit) };
        }

        private List<string> GetProductStrings(string html)
        {
            int currentIndex = html.IndexOf("<header id=\"coop-nav");
            if (currentIndex == -1) throw new Exception("Error retrieving product string cannot find the header id");

            List<string> productStrings = new List<string>();

            while (true)
            {
                int startIndex = GetIndexBackSearch(html, currentIndex);
                if (startIndex == -1)
                {
                    break; // No more products found, exit loop
                }

                // Search for the next product after `startIndex + 50`
                int endIndex = GetIndexBackSearch(html, startIndex + 50);

                if (endIndex == -1)
                {
                    // No more products, handle the last product case
                    string lastProductString = GetOnlyProductStringFromLastElement(html);
                    productStrings.Add(lastProductString);
                    break;
                }

                // Extract the product string using indices without Substring
                productStrings.Add(html.Substring(startIndex, endIndex - startIndex));

                // Move currentIndex forward to the end of the current product
                currentIndex = endIndex;
            }

            return productStrings;
        }

        private string GetOnlyProductStringFromLastElement(string productString)
        {
            int currentIndex = 0;
            int divStack = 0;
            int latestClosingDiv = 0;
            bool reachedEnd = false;

            while (!reachedEnd && currentIndex < productString.Length)
            {
                int nextStartingDiv = productString.IndexOf("<div", currentIndex);
                int nextClosingDiv = productString.IndexOf("</div>", currentIndex);

                // Check which comes first, opening or closing div
                if (nextStartingDiv != -1 && (nextStartingDiv < nextClosingDiv || nextClosingDiv == -1))
                {
                    divStack++;
                    currentIndex = nextStartingDiv + 4; 
                }
                else if (nextClosingDiv != -1)
                {
                    divStack--;
                    currentIndex = nextClosingDiv + 6;
                    latestClosingDiv = currentIndex;
                }
                else
                {
                    // No more div tags found
                    reachedEnd = true;
                }

                if (divStack == 0)
                {
                    return productString.Substring(0, latestClosingDiv);
                }
            }

            throw new Exception("No valid div structure found"); 
        }
        private int GetIndexBackSearch(string html, int startPosition = 0)
        {
            int startIndex = html.IndexOf("data-role=\"offer", startPosition);
            if (startIndex == -1)
            {
                return -1; // Return early if no match is found
            }
            return html.LastIndexOf("<div data-id=\"", startIndex);
        }

    }
}
