using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public async Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId)
        {
            var response = await CallUrl(ProductsLocationUrl, 5000);
            List<Product> products = new List<Product>();
            List<string> productStrings = GetProductStrings(response);

            //File.WriteAllText("response.text", response);

            for (int i = 0; i < productStrings.Count; i++)
            {
                try
                {
                    progressCallback((int)(((double)i / productStrings.Count) * 100));
                    List<Product>? innerProducts = CreateProducts(productStrings[i], companyId);
                    if (innerProducts != null)
                    {
                        products.AddRange(innerProducts);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return products;
        }

        private static (Dictionary<string, int>, bool) CountStringOccurrencesAndCheck(string[] inputArray)
        {
            Dictionary<string, int> stringCounts = new Dictionary<string, int>();
            bool hasMoreThanTwo = false;

            foreach (string value in inputArray)
            {
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

        private static List<Product>? CreateProducts(string productContainedHtml, int companyId)
        {

            List<Product> products = new List<Product>();

            string productInformation = GetInformationFromHtml<string>(productContainedHtml, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");

            string name = RemoveUselessPartsString(GetNameFromHtml(productContainedHtml));

            if(name.Contains("Carlsberg eller Tuborg"))
            {
                Debug.WriteLine("BReak");
            }

            string description = GetDescriptionFromHtml(productInformation);
            string imageUrl = GetImageUrlFromHtml(productContainedHtml);



            string[] compareUnitsInDescription = GetUnitsFromDescription(description);

            List<Price>? prices = CreatePrices(productContainedHtml, GetCompareUnit(description));

            if (prices == null)
            {
                // Ignore the product if it has no prices or if they cant be found
                return null;
            }

            string externalId = GetExternalIdFromProductContainedHtml(productContainedHtml);

            // If it has two of the same compare units then its likely to have another product in the description.
            // Next up is checking if it has two compare units that measure different things like ml and g

            var (units, hasMoreThanTwo) = CountStringOccurrencesAndCheck(compareUnitsInDescription);

            if (hasMoreThanTwo)
            {
                Debug.WriteLine(description + " Is believed to have more than two");
                foreach (var unit in units)
                {
                    int count = unit.Value;
                    if (count > 1)
                    {
                        int indexOffSet = 0;
                        for (int i = 0; i < count; i++)
                        {
                            float amountOfProductInTheProduct = GetAmountOfProductFromDescription(description, indexOffSet);

                            indexOffSet += GetIndexOfAmountFromDescription(description, amountOfProductInTheProduct, indexOffSet);

                            float amount = IProductScraper.GetAmountInProduct(amountOfProductInTheProduct, unit.Key, prices);

                            Product product = new Product(prices, name, imageUrl, description, externalId + "---" + i, amount, companyId);
                            products.Add(product);
                        }
                    }
                }
            }
            else
            {
                float amount = 0;

                if (compareUnitsInDescription.Length == 0)
                {
                    // Check for edgecase where prices stored in half kilo has no unit except for the compare unit
                    // If it is found set amount to 0.5 
                    if (prices[0].CompareUnitString.Equals("kg"))
                    {
                        amount = 0.5f;
                    }
                    else if(prices[0].CompareUnitString.Equals("stk") || prices[0].CompareUnitString.Equals("bdt"))
                    {
                        amount = 1;
                    }
                    else
                    {
                        throw new Exception("No compare unit found in description: " + description);
                    }
                }
                else
                {
                    string productInUnit = compareUnitsInDescription[0];

                    float amountOfProductInTheProduct = GetAmountOfProductFromDescription(description);

                    amount = IProductScraper.GetAmountInProduct(amountOfProductInTheProduct, productInUnit, prices);
                }

                Product product = new Product(prices, name, imageUrl, description, externalId, amount, companyId);
                products.Add(product);
            }
            return products;
        }

        private static int GetIndexOfAmountFromDescription(string description, float amount, int indexOffset = 0)
        {
            int index = description.IndexOf(amount.ToString(), indexOffset);
            if (index != -1)
            {
                return index;
            }
            return 0;
        }

        private static float GetAmountOfProductFromDescription(string description, int startIndex= 0)
        {
            // first try parse until a " " is found
            // Then go to the next / and parse to " " again
            // Example 300 cl./627 cl.

            int firstIndex = description.IndexOf(" ", startIndex);
            if (firstIndex == -1)
            {
                throw new Exception("Cant locate amount in the description: " + description);
            }

            string firstPart = description.Substring(startIndex, firstIndex);

            if (float.TryParse(firstPart, out float firstAmount))
            {
                return firstAmount;
            }
            throw new Exception("Cant parse amount: " + firstPart + " in the description: " + description);

        }

        private static string GetImageUrlFromHtml(string productContainedHtml)
        {
            string url = GetInformationFromHtml<string>(productContainedHtml, "=\"https://image-transformer-api.tjek.com/", "\"", "\"");
            url = url.Replace("amp;", "");
            return url;
        }

        private static string[] GetUnitsFromDescription(string description)
        {
            List<string> units = new List<string>();

            string[] possibleUnitsToLookFor = { "g", "cl", "ml", "stk", "kg", "liter" };

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
            char[] validSurroundings = { ' ', '.', '/', '\0' };

            if (index > 0 && !Array.Exists(validSurroundings, c => c == input[index - 1]))
            {
                return false;
            }

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
                description = ProcessDescription(description, '-', ChangeEstimateToAverage);
                description = ProcessDescription(description, 'x', ChangeMultiplyToOneValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return RemoveUselessPartsString(description);
        }

        private static string ProcessDescription(string description, char symbol, Func<string, int, char, string> processFunc)
        {
            int location = description.IndexOf(symbol, StringComparison.OrdinalIgnoreCase);

            while (location != -1)
            {
                // Update the description using the processFunc for the current location of the symbol
                description = processFunc(description, location, symbol);

                // Find the next occurrence of the symbol after the current position
                location = description.IndexOf(symbol, location + 1);
            }

            return description;
        }

        private static string RemoveUselessPartsString(string description)
        {
            // remove min, formatting, "maks.", any double empty, " + pant."
            description = description.Replace("Min.", "").Replace("&nbsp;", " ").Replace("&amp;", "").Replace("maks.", "").Replace("  ", " ").Replace(" + pant.", "");
            if (description.StartsWith(" "))
            {
                description = description.Substring(1);
            }
            return description;
        }

        private static float GetPriceFromHtml(string productContainedHtml)
        {
            try { return GetInformationFromHtml<float>(productContainedHtml, ",-", ">", ",", -10); } catch { return -1; }
        }

        private static string GetCompareUnit(string stringToExtractFrom)
        {
            string[] possibleUnits = { "Stk-pris", "Kg-pris", "Literpris", "Bdt.-pris"};
            foreach (string unit in possibleUnits)
            {
                if (stringToExtractFrom.Contains(unit))
                {
                    string newString = unit.Replace("pris", "").Replace(".-", "").Replace("-", "").Replace("Liter", "ltr").ToLower();
                    Debug.WriteLine(newString);
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

        private static List<Price>? CreatePrices(string productContainedHtml, string compareUnit)
        {
            float price = GetPriceFromHtml(productContainedHtml);
            if (price != -1)
            {
                return new List<Price> { new Price(price, compareUnit) };
            }
            else return null;

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

        // Find the index of the closing div and substring the product information
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
                    // No div tags found
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