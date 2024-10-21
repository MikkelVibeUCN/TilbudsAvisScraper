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

            File.WriteAllText("response.text", response);
            //int i = 0;
            //foreach (var productString in productStrings)
            //{
            //    string fileName = $"{productString.Length} count {i}.text";
            //    Debug.WriteLine(fileName);
            //    File.WriteAllText(fileName, productString);
            //    i++;
            //}
            // TODO: ProductString seems to be double of the same product sometimes cant debug website is down
            foreach (var productString in productStrings)
            {
                //Product product = CreateProduct(productString);
                //products.Add(product);
                string productInformation = GetInformationFromHtml<string>(productString, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");
                Debug.WriteLine(GetDescriptionFromHtml(productInformation));
            }
            return products;
        }

        public Product CreateProduct(string productContainedHtml)
        {
            string productInformation = GetInformationFromHtml<string>(productContainedHtml, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");

            string name = GetNameFromHtml(productContainedHtml);
            string description = GetDescriptionFromHtml(productInformation);
            float price = GetPriceFromHtml(productContainedHtml);
            string compareUnit = GetCompareUnitFromDescription(description);
            int externalId = GetExternalIdFromProductContainedHtml(productContainedHtml);


            List<Price> prices = CreatePrices(productContainedHtml, compareUnit);

            float amount = CalculateAmount(price, compareUnit, description);

            return new Product(prices, null, name, "", description, -1, null, amount);

            throw new NotImplementedException();
        }

        private string GetNameFromHtml(string productHtml)
        {
            string nameOfProduct = GetInformationFromHtml<string>(productHtml, "data-role=\"productInformation\"", "class=\"incito__view incito__text-view\">", "<");
            nameOfProduct.Replace("&nbsp;", " ");
            nameOfProduct.Replace("*", "");
            return nameOfProduct;
        }

        private string GetDescriptionFromHtml(string productInformation)
        {
            string searchString = "class=\"incito__view incito__text-view\">";
            int firstInformationIndex = productInformation.IndexOf(searchString);
            string description = "";
            try
            {
                description = GetInformationFromHtml<string>(productInformation, "class=\"incito__view incito__text-view\"", ">", ".", firstInformationIndex);
            }
            catch
            {
                Debug.WriteLine("Failed to get description, most likely doesnt have one");
            }
            return description;
        }

        private float GetPriceFromHtml(string productContainedHtml)
        {
            return GetInformationFromHtml<float>(productContainedHtml, ",-", ">", ",", -10);
        }

        private string GetCompareUnitFromDescription(string description)
        {
            if(description.Equals(string.Empty))
            {
                return "";
            }

            string[] possibleUnits = { "kg", "g", "l", "cl", "ml", "stk", "bakke" };



            throw new NotImplementedException();
        }

        private int GetExternalIdFromProductContainedHtml(string productContainedHtml)
        {
            return GetInformationFromHtml<int>(productContainedHtml, "data-id=", "\"", "\"");
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
            // Start looking for the coop-nav header to ensure we are within the correct section
            int currentIndex = html.IndexOf("<header id=\"coop-nav");
            if (currentIndex == -1) throw new Exception("Error retrieving product string - cannot find the header id");

            List<string> productStrings = new List<string>();

            // Start looking for "data-role=\"offer\"" from the current index
            while (true)
            {
                // Find the next occurrence of the offer block
                int offerIndex = html.IndexOf("data-role=\"offer\"", currentIndex);
                if (offerIndex == -1)
                {
                    // No more offer blocks found, break the loop
                    break;
                }

                // Now we need to find the starting <div that has the "data-role=\"offer\""
                int startingDivIndex = html.LastIndexOf("<div", offerIndex);
                if (startingDivIndex == -1)
                {
                    throw new Exception("Error retrieving product string - cannot find the starting div for the offer");
                }

                // Extract the substring that starts at the <div
                string partialHtml = html.Substring(startingDivIndex);

                // Use GetOnlyProductString to retrieve the valid HTML structure for this product
                string productString = GetOnlyProductString(partialHtml);

                // Add the valid product HTML to the result list
                productStrings.Add(productString);

                // Move current index past this product for the next iteration
                currentIndex = startingDivIndex + productString.Length;
            }

            return productStrings;
        }



        private string GetOnlyProductString(string productString)
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
