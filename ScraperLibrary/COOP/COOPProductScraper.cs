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
                List<Product> innerProducts = CreateProducts(productString);
                //foreach (var product in innerProducts)
                //{
                //    products.Add(product);
                //}
                //string productInformation = GetInformationFromHtml<string>(productString, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");
                //Debug.WriteLine("Name: " + GetNameFromHtml(productString));
                //string description = GetDescriptionFromHtml(productInformation);
                //Debug.WriteLine("Description: " + description);
                //string[] units = GetUnitsFromDescription(description);
                //Debug.Write("Units found in description: ");
                //foreach (var unit in units)
                //{
                //    Debug.Write(unit + ", ");
                //}
                //Debug.WriteLine("");
            }
            return products;
        }

        private static (Dictionary<string, int>, bool) CountStringOccurrencesAndCheck(string[] inputArray)
        {
            Dictionary<string, int> stringCounts = new Dictionary<string, int>();
            bool hasMoreThanTwo = false;

            foreach (string value in inputArray)
            {
                // If the string already exists in the dictionary, increment the count
                if (stringCounts.ContainsKey(value))
                {
                    stringCounts[value]++;
                }
                else
                {
                    stringCounts[value] = 1;
                }

                if (stringCounts[value] > 1)
                {
                    hasMoreThanTwo = true;
                }
            }
            return (stringCounts, hasMoreThanTwo);
        }



        private static List<Product> CreateProducts(string productContainedHtml)
        {
            List< Product> products = new List< Product>();

            string productInformation = GetInformationFromHtml<string>(productContainedHtml, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");

            string name = GetNameFromHtml(productContainedHtml);
            string description = GetDescriptionFromHtml(productInformation);
            string imageUrl = GetImageUrlFromHtml(productContainedHtml);

            string[] CompareUnitsInDescription = GetUnitsFromDescription(description);

            List<Price> prices = CreatePrices(productContainedHtml, GetCompareUnit(description));

            // This needs to run for every product
            string externalId = GetExternalIdFromProductContainedHtml(productContainedHtml);

            // If it has two of the samse compare units then its likely to have another product in the description
            var (units, hasMoreThanTwo) = CountStringOccurrencesAndCheck(CompareUnitsInDescription);
            if (hasMoreThanTwo)
            {
                Console.WriteLine("");
                foreach (var unit in units)
                {
                    int amount = unit.Value;
                    if (amount > 1)
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            // Add product here
                        }
                    }
                }
            }
            else
            {

                Product product = new Product(prices, name, imageUrl, description, externalId, IProductScraper.GetAmountInProduct(description, prices, );
                products.Add(product);
            }
            
            

            return null;

            //List<Price> prices = CreatePrices(productContainedHtml, compareUnit);
            //
            //float amount = CalculateAmount(price, compareUnit, description);
            //
            //return new Product(prices, null, name, "", description, -1, null, amount);
            //
            //throw new NotImplementedException();
        }

        private static string GetImageUrlFromHtml(string productContainedHtml)
        {
            string url = GetInformationFromHtml<string>(productContainedHtml, "data-bg=\"", "\"", "\"");
            url.Replace("amp;", "");
            return url;
        }

        private static string[] GetUnitsFromDescription(string description)
        {
            List<string> units = [];

            string[] possibleUnitsToLookFor = { "g", "kg", "cl", "liter", "ml", "stk" };


            foreach (string unit in possibleUnitsToLookFor)
            {
                // Search for the unit in the input string
                int index = description.IndexOf(unit, StringComparison.OrdinalIgnoreCase);

                // Continue searching for all occurrences of the unit
                while (index != -1)
                {
                    // Check if the surrounding characters are valid
                    if (IsValidUnitPosition(description, unit, index))
                    {
                        units.Add(unit);
                    }

                    // Continue searching for more occurrences of the unit
                    index = description.IndexOf(unit, index + unit.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
            return units.ToArray();
        }

        private static bool IsValidUnitPosition(string input, string unit, int index)
        {
            // Define valid surrounding characters (space, dot, numbers, or nothing)
            char[] validSurroundings = { ' ', '.', '/', '\0' }; // Add any additional valid characters as needed

            // Check the character before the unit
            if (index > 0 && !Array.Exists(validSurroundings, c => c == input[index - 1]))
            {
                return false;
            }

            // Check the character after the unit and return the inverted condition
            int endIndex = index + unit.Length;
            return endIndex == input.Length || Array.Exists(validSurroundings, c => c == input[endIndex]);
        }


        private static string GetNameFromHtml(string productHtml)
        {
            string nameOfProduct = GetInformationFromHtml<string>(productHtml, "data-role=\"productInformation\"", "class=\"incito__view incito__text-view\">", "<");
            nameOfProduct.Replace("&nbsp;", " ");
            nameOfProduct.Replace("*", "");
            return nameOfProduct;
        }

        private static string GetDescriptionFromHtml(string productInformation)
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
        private static string RemoveUselessPartsOfDescription(string description)
        {
            // remove min, formatting, "maks.", any double empty, " + pant."
            description = description.Replace("Min.", "").Replace("&nbsp;", " ").Replace("maks.", "").Replace("  ", " ").Replace(" + pant.", "");
            if (description.StartsWith(" "))
            {
                description = description.Substring(1);
            }
            return description;
        }

        private static float GetPriceFromHtml(string productContainedHtml)
        {
            return GetInformationFromHtml<float>(productContainedHtml, ",-", ">", ",", -10);
        }

        private static string GetCompareUnit(string stringToExtractFrom)
        {
            string[] possibleUnits = { "Stk-pris", "Kg-pris", "Literpris" };
            foreach (string unit in possibleUnits)
            {
                if (stringToExtractFrom.Contains(unit))
                {
                    string newString = unit.Replace("pris", "").Replace("-", "").Replace("iter", "").ToLower();
                    return newString;
                }
            }
            return "";
        }

        private static string GetExternalIdFromProductContainedHtml(string productContainedHtml)
        {
            return GetInformationFromHtml<string>(productContainedHtml, "data-id=", "\"", "\"");
        }

        private float CalculateAmount(float price, string compareUnit, string description)
        {
            throw new NotImplementedException();
        }

        private static List<Price> CreatePrices(string productContainedHtml, string compareUnit)
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