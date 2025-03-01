using TilbudsAvisLibrary.DTO;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using System.ComponentModel.Design;

namespace ScraperLibrary.COOP
{
    public class COOPProductScraper : Scraper, IProductScraper
    {
        private string ProductsLocationUrl { get; set; }
        public COOPProductScraper(string productLocationsUrl)
        {
            ProductsLocationUrl = productLocationsUrl;
        }

        public async Task<List<ProductDTO>> GetAllProductsFromPage(
            Action<int> progressCallback,
            CancellationToken token,
            string avisExternalId,
            int companyId)
        {
            var response = await CallUrl(ProductsLocationUrl, 5000);
            List<ProductDTO> products = new List<ProductDTO>();
            List<string> offerStrings = GetOfferStrings(response);
            HashSet<string> addedExternalIds = new HashSet<string>();

            for (int i = 0; i < offerStrings.Count; i++)
            {
                try
                {
                    progressCallback((int)(((double)i / offerStrings.Count) * 100));
                    List<ProductDTO>? innerProducts = ConvertOfferToProducts(offerStrings[i], companyId, avisExternalId);

                    if (innerProducts != null) products.AddRange(innerProducts);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return products;
        }

        private static List<ProductDTO>? ConvertOfferToProducts(string offerContainedHtml, int companyId, string externalAvisId)
        {
            List<ProductDTO> products = new List<ProductDTO>();

            var (name, description, imageUrl, compareUnitsInDescription, prices, externalId) = ExtractBasicProductInfo(offerContainedHtml, externalAvisId);

            if (prices == null)
            {
                return null;
            }

            var (units, hasMoreThanTwo) = CountUnitOccurrencesAndCheckForMultiple(compareUnitsInDescription);

            if (hasMoreThanTwo)
            {
                ProcessMultipleProductOffer(units, description, prices, name, imageUrl, externalId, products);
            }
            else
            {
                ProcessSingleProductOffer(compareUnitsInDescription, prices, description, name, imageUrl, externalId, products);
            }
            return products;
        }

        private static (string name, string description, string imageUrl, string[] compareUnitsInDescription, List<PriceDTO>? prices, string externalId) ExtractBasicProductInfo(string offerContainedHtml, string externalAvisId)
        {
            string name = RemoveUselessPartsString(GetNameFromHtml(offerContainedHtml)).ToUpper();
            string imageUrl = GetImageUrlFromHtml(offerContainedHtml);
            string externalId = GetExternalIdFromOfferHtml(offerContainedHtml);
            string description = CreateDescriptionFromHtml(offerContainedHtml).ToUpper();

            string[] compareUnitsInDescription = GetUnitsFromDescription(description);

            List<PriceDTO>? prices = CreatePrices(offerContainedHtml, GetCompareUnit(description), externalAvisId);

            return (name, description, imageUrl, compareUnitsInDescription, prices, externalId);
        }

        private static (Dictionary<string, int>, bool) CountUnitOccurrencesAndCheckForMultiple(string[] inputArray)
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

        

        private static void ProcessMultipleProductOffer(Dictionary<string, int> units, string description, List<PriceDTO> prices, string name, string imageUrl, string externalId, List<ProductDTO> products)
        {
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

                        ProductDTO product = new ProductDTO
                        {
                            Prices = prices,
                            Name = name,
                            ImageUrl = imageUrl,
                            Description = description,
                            ExternalId = externalId + "---" + i,
                            Amount = amount,
                        };
                        products.Add(product);
                    }
                }
            }
        }

        private static void ProcessSingleProductOffer(string[] compareUnitsInDescription, List<PriceDTO> prices, string description, string name, string imageUrl, string externalId, List<ProductDTO> products)
        {
            float amount = 0;

            if (compareUnitsInDescription.Length == 0)
            {
                // Check for edgecase where prices stored in half kilo has no unit except for the compare unit
                // If it is found set amount to 0.5 
                if (prices[0].CompareUnit.Equals("kg"))
                {
                    amount = 0.5f;
                }
                else if (prices[0].CompareUnit.Equals("stk") || prices[0].CompareUnit.Equals("bdt"))
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

            ProductDTO product = new ProductDTO
            {
                Prices = prices,
                Name = name,
                ImageUrl = imageUrl,
                Description = description,
                ExternalId = externalId,
                Amount = amount,
            };
            products.Add(product);
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

        private static float GetAmountOfProductFromDescription(string description, int startIndex = 0)
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

        private static string GetImageUrlFromHtml(string offerContainedHtml)
        {
            string url = GetInformationFromHtml<string>(offerContainedHtml, "=\"https://image-transformer-api.tjek.com/", "\"", "\"");
            url = url.Replace("amp;", "");
            return url;
        }

        private static string[] GetUnitsFromDescription(string description)
        {
            List<string> units = new List<string>();

            string[] possibleUnitsToLookFor = { "g", "cl", "ml", "stk", "kg", "liter" };

            foreach (string unit in possibleUnitsToLookFor)
            {
                int index = description.IndexOf(unit, StringComparison.OrdinalIgnoreCase);

                while (index != -1)
                {
                    if (IsValidUnitPosition(description, unit, index))
                    {
                        units.Add(unit);
                    }

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
            nameOfProduct = nameOfProduct.Replace("&nbsp;", " ")
                .Replace("&amp; ", "")
                .Replace("*", "");
            return nameOfProduct;
        }

        private static string CreateDescriptionFromHtml(string offerContainedHtml)
        {
            // altid gang to tal sammen hvis der er et x imellem to tal DONE
            // hvis der er - så tag gennemsnittet og brug det DONE
            // Hvis der er / er der tale om to produkter og context er derfor nødtvendigt TODO:
            // det ligner at hvis der står to produktnavne i navnefældet så står de i rækkefølge.
            // Tå tjek hvor mange navne der er ved at lede efter "eller", hvis der ikke er nogen eller så gem to produkter en med hver mængde
            // altid fjern "min" fra description DONE

            string basicProductInformation = GetInformationFromHtml<string>(offerContainedHtml, "data-role=\"productInformation\"", "class=\"incito__view\">", "</div>");

            string searchString = "class=\"incito__view incito__text-view\">";
            int firstInformationIndex = basicProductInformation.IndexOf(searchString);
            string description = "No description available";
            try
            {
                description = GetInformationFromHtml<string>(basicProductInformation, "class=\"incito__view incito__text-view\"", ">", "<", firstInformationIndex);

                // Check for "-" and "x" in the description
                description = ProcessDescription(description, '-', ChangeEstimateToAverage);
                description = ProcessDescription(description, 'x', ChangeMultiplyToOneValue);

                description = RemovePriceRelatedParts(description);
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

        private static string RemovePriceRelatedParts(string description)
        {
            string[] priceRelatedParts = { "Kg-pris", "Liter-pris", "Literpris", "Stk-pris" };

            foreach (string part in priceRelatedParts)
            {
                int index = description.IndexOf(part, StringComparison.OrdinalIgnoreCase);
                if (index == 0)
                {
                    if (description.Contains("Kg-pris"))
                    {
                        description = "1 KG.";
                    }
                    else if (description.Contains("Liter-pris") || description.Contains("Literpris"))
                    {
                        description = "1 LTR.";

                    }
                    else if (description.Contains("Stk-pris"))
                    {
                        description = "1 STK.";
                    }
                }
                else if (index != -1)
                {
                    description = description.Substring(0, index);
                }
            }
            return description;
        }

        private static string RemoveUselessPartsString(string description)
        {
            // remove min, formatting, "maks.", any double empty, " + pant."
            description = description.Replace("Min.", "")
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "")
                .Replace("maks.", "")
                .Replace("  ", " ")
                .Replace(" + pant.", "")
                .Replace("*", "");

            if (description.StartsWith(" "))
            {
                description = description.Substring(1);
            }
            if (description.EndsWith(" "))
            {
                description = description.Substring(0, description.Length - 1);
            }
            return description;
        }

        private static float GetPriceFromHtml(string offerContainedHtml)
        {
            try
            {
                if (offerContainedHtml.Contains("superscript"))
                {
                    float wholeNumber = GetInformationFromHtml<float>(offerContainedHtml, "data-single-line", "class=\"incito__view incito__text-view\">", "<");

                    float secondNumber = GetInformationFromHtml<float>(offerContainedHtml, "superscript", ">", "<");

                    return CombineFloats(wholeNumber, secondNumber);
                }
                else
                {
                    return GetInformationFromHtml<float>(offerContainedHtml, ",-", ">", ",", -10);
                }
            }
            catch
            {
                return -1;
            }
        }

        private static string GetCompareUnit(string stringToExtractFrom)
        {
            string[] possibleUnits = { "g", "kg", "ltr", "ml", "cl", "bakke", "stk" };
            foreach (string unit in possibleUnits)
            {
                if (stringToExtractFrom.ToLower().Contains(unit))
                {
                    if (unit.Equals(possibleUnits[0]) || unit.Equals(possibleUnits[1]))
                    {
                        return "kg";
                    }
                    else if (unit.Equals(possibleUnits[2]) || unit.Equals(possibleUnits[3]) || unit.Equals(possibleUnits[4]))
                    {
                        return "ltr";
                    }
                    else if (unit.Equals(possibleUnits[5]) || unit.Equals(possibleUnits[6]))
                    {
                        return "stk";
                    }
                }
            }
            return "";
        }

        private static string GetExternalIdFromOfferHtml(string offerContainedHtml)
        {
            return GetInformationFromHtml<string>(offerContainedHtml, "data-id=", "\"", "\"");
        }

        private float CalculateAmount(float price, string compareUnit, string description)
        {
            throw new NotImplementedException();
        }

        private static List<PriceDTO>? CreatePrices(string offerContainedHtml, string compareUnit, string externalAvisId)
        {
            float price = GetPriceFromHtml(offerContainedHtml);
            if (price != -1)
            {
                return new List<PriceDTO>
                {
                    new PriceDTO
                    {
                        Price = price,
                        CompareUnit = compareUnit,
                        ExternalAvisId = externalAvisId
                    }
                };

            }
            else return null;

        }

        private List<string> GetOfferStrings(string html)
        {
            int currentIndex = html.IndexOf("<header id=\"coop-nav");
            if (currentIndex == -1) throw new Exception("Error retrieving product strings - cannot find the header id");

            List<string> productStrings = new List<string>();

            while (true)
            {
                int offerIndex = html.IndexOf("data-role=\"offer\"", currentIndex);
                if (offerIndex == -1)
                {
                    break;
                }

                int startingDivIndex = html.LastIndexOf("<div", offerIndex);
                if (startingDivIndex == -1)
                {
                    throw new Exception("Error retrieving product string - cannot find the starting div for the offer");
                }
                string partialHtml = html.Substring(startingDivIndex);

                string productString = GetOnlyProductString(partialHtml);

                productStrings.Add(productString);

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
                    reachedEnd = true;
                }

                if (divStack == 0)
                {
                    return productString.Substring(0, latestClosingDiv);
                }
            }
            throw new Exception("No valid div structure found");
        }
        
        private static float CombineFloats(float wholeNumber, float secondNumber)
        {
            float decimalPart = secondNumber / (float)Math.Pow(10, secondNumber.ToString().Length);
            return wholeNumber + decimalPart;
        }
    }
}