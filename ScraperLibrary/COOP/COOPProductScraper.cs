using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP
{
    public class COOPProductScraper : Scraper, IProductScraper
    {
        private string ProductsLocationUrl { get; set; }
        public COOPProductScraper(string productLocationsUrl)
        {
            ProductsLocationUrl = productLocationsUrl;
        }

        public async Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId)
        {
            var response = await CallUrl(ProductsLocationUrl, 5000);
            List<Product> products = new List<Product>();
            List<string> productStrings = GetProductStrings(response);

            // File.WriteAllText("response.text", response);

            foreach (var productString in productStrings)
            {
                Product product = CreateProduct(productString);
                products.Add(product);
                //string productInformation = GetInformationFromHtml<string>(productString, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");
                //Debug.WriteLine(GetDescriptionFromHtml(productInformation));
            }
            return products;
        }

        private Product CreateProduct(string productContainedHtml)
        {
            string productInformation = GetInformationFromHtml<string>(productContainedHtml, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");

            string name = GetNameFromHtml(productContainedHtml);
            string description = GetDescriptionFromHtml(productInformation);
            float price = GetPriceFromHtml(productContainedHtml);
            string[] compareUnits = GetCompareUnitsFromDescription(description);
            int externalId = GetExternalIdFromProductContainedHtml(productContainedHtml);

            // TODO: implement when multiple products are bundles togetherw ith one price and split them up

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
            // altid gang to tal sammen hvis der er et x imellem to tal DONE
            // hvis der er - så tag gennemsnittet og brug det DONE
            // Hvis der er / er der tale om to produkter og context er derfor nødtvendigt TODO:
            // det ligner at hvis der står to produktnavne i navnefældet så står de i rækkefølge.
            // Tå tjek hvor mange navne der er ved at lede efter "eller", hvis der ikke er nogen eller så gem to produkter en med hver mængde
            // altid fjern "min" fra description DONE

            string searchString = "class=\"incito__view incito__text-view\">";
            int firstInformationIndex = productInformation.IndexOf(searchString);
            string description = "No description available";
            try
            {
                description = GetInformationFromHtml<string>(productInformation, "class=\"incito__view incito__text-view\"", ">", "<", firstInformationIndex);

                // Check for "-" and "x" in the description
                ProcessDescription(ref description, '-', ChangeEstimatesToAverage);
                ProcessDescription(ref description, 'x', ChangeMultiplyToOneValue);
            }
            catch
            {
            
            }
            return RemoveUselessPartsOfDescription(description);
        }
        private static void ProcessDescription(ref string description, char symbol, Func<string, int, char, string> processFunc)
        {
            int location = description.IndexOf(symbol, StringComparison.OrdinalIgnoreCase);
            if (location != -1)
            {
                description = processFunc(description, location, symbol);
            }
        }
        private string RemoveUselessPartsOfDescription(string description)
        {
            // remove min, formatting, "maks.", any double empty, " + pant."

            description = description.Replace("Min.", "").Replace("&nbsp;", " ").Replace("maks.", "").Replace("  ", " ").Replace(" + pant.", "");
            if (description.StartsWith(" "))
            {
                description = description.Substring(1);
            }
            return description;
        }

        private float GetPriceFromHtml(string productContainedHtml)
        {
            return GetInformationFromHtml<float>(productContainedHtml, ",-", ">", ",", -10);
        }

        private string[] GetCompareUnitsFromDescription(string description)
        {
            string[] returnList = [];
            string[] possibleUnits = { "Stk-pris", "Kg-pris", "Literpris" };
            foreach (string unit in possibleUnits)
            {
                if (description.Contains(unit))
                {
                    string newString = unit.Replace("pris", "").Replace("-", "").Replace("iter", "").ToLower();
                    returnList.Append(newString);
                }
            }
            return returnList;
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
            int currentIndex = html.IndexOf("<header id=\"coop-nav");
            if (currentIndex == -1) throw new Exception("Error retrieving product strings - cannot find the header id");

            List<string> productStrings = new List<string>();

            while (true)
            {
                // Find the next occurrence
                int offerIndex = html.IndexOf("data-role=\"offer\"", currentIndex);
                if (offerIndex == -1)
                {
                    // No more offer blocks found, break the loop
                    break;
                }

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