using Newtonsoft.Json.Linq;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Lidl
{
    public class LidlProductScraper : Scraper, IProductScraper
    {
        private CultureInfo locale = new CultureInfo("da-DK");
        public Task<List<ProductDTO>> GetAllProductsFromPage(
            Action<int> progressCallback,
            CancellationToken token,
            string avisExternalId,
            int companyId,
            dynamic JSON = null)
        {
            List<ProductDTO> products = new();



            // Cast JSON to JObject to get key-value pairs
            var jsonObject = JSON as JObject;
            if (jsonObject == null)
                throw new ArgumentException("Invalid JSON format.");

            var entries = jsonObject.Properties().ToList();

            for (int i = 0; i < entries.Count; i++)
            {

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                var productData = entries[i].Value;

                try
                {
                    ProductDTO newProduct = CreateProduct(productData, avisExternalId);

                    products.Add(newProduct);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing product at index {i}: {ex.Message}");
                    // Optionally, you can log the error or handle it as needed
                }



                int progress = (int)(((double)(i + 1) / entries.Count) * 100);
                progressCallback(progress);
            }

            return Task.FromResult(products);
        }



        private ProductDTO CreateProduct(dynamic productJSON, string avisExternalId)
        {
            // Get the easy ones first like id, price, imageUrl and title

            string id = productJSON.productId;

            string name = productJSON.title;

            if (name.Equals("LUPILU® Bleer tubes"))
            {
                Console.WriteLine();
            }


            string imageUrl = productJSON.image;

            float price = productJSON.price;

            // Find compare unit and price per said unit
            string unprocessedDescription = productJSON.basicPrice;




            (string compareUnit, float pricePerUnit) = FindCompareUnitAndPrice(unprocessedDescription, price);

            // Calculate the amount based on the price and compare unit
            float amount = CalculateAmount(price, pricePerUnit);

            string description = CreateDescription(amount, compareUnit);

            File.AppendAllText("ProductNames.txt", unprocessedDescription + " After: " + description + "(price " + price + ")" + Environment.NewLine);

            // Create prices
            List<PriceDTO> prices = CreatePrices(avisExternalId, compareUnit, price, productJSON.strokePrice);

            return new ProductDTO
            {
                ImageUrl = imageUrl,
                ExternalId = id,
                Name = name,
                Amount = amount,
                Prices = prices,
                Description = CreateDescription(amount, compareUnit),
            };
        }

        public PriceDTO CreatePrice(string externalAvisId, string compareUnit, float price)
        {
            return new PriceDTO
            {
                ExternalAvisId = externalAvisId,
                CompareUnit = compareUnit,
                Price = price,
            };
        }

        private List<PriceDTO> CreatePrices(string externalAvisId, string compareUnit, float price, dynamic strokePrice)
        {

            List<PriceDTO> prices = [CreatePrice(externalAvisId, compareUnit, price)];

            // If product has strokePrice its the same as a base price so save it to the companies base
            if (strokePrice != null)
            {
                string strokePriceText = (string)strokePrice.Value;

                prices.Add(CreatePrice("base", compareUnit, float.Parse(strokePriceText.Replace("-", ""), locale)));
            }

            return prices;
        }

        private string CreateDescription(float amount, string compareUnit)
        {
            const float KG_TO_GRAMS = 1000f;
            const float LITER_TO_CL = 100f;
            const float MIN_KG_THRESHOLD = 1f;
            const float MIN_LITER_THRESHOLD = 1f;

            return compareUnit.ToUpperInvariant() switch
            {
                "KG" => CreateWeightDescription(amount),
                "LITER" => CreateVolumeDescription(amount),
                "STK" => CreateCountDescription(amount),
                // Handle legacy units that might still come through
                "LTR" => CreateVolumeDescription(amount),
                "GR" => $"{FormatAmount(amount)} GR.", // Direct gram formatting - don't convert
                "CL" => $"{FormatAmount(amount)} CL.", // Direct cl formatting - don't convert
                _ => CreateCountDescription(amount) // Default everything else to STK
            };

            string CreateWeightDescription(float kg)
            {
                if (kg >= MIN_KG_THRESHOLD)
                {
                    return $"{FormatAmount(kg)} KG.";
                }

                var grams = Math.Round(kg * KG_TO_GRAMS, 0);
                return $"{FormatAmount((float)grams)} GR.";
            }

            string CreateVolumeDescription(float liters)
            {
                if (liters >= MIN_LITER_THRESHOLD)
                {
                    return $"{FormatAmount(liters)} LTR.";
                }

                var centiliters = Math.Round(liters * LITER_TO_CL, 2);
                return $"{FormatAmount((float)centiliters)} CL.";
            }

            string CreateCountDescription(float count)
            {
                return $"{FormatAmount(count)} STK.";
            }

            string FormatAmount(float value)
            {
                var rounded = (float)Math.Round(value, 2);

                return rounded % 1 == 0
                    ? ((int)rounded).ToString()
                    : rounded.ToString("0.##", CultureInfo.InvariantCulture);
            }
        }

        private float CalculateAmount(float pricePer, float pricePerUnit)
        {
            // Based on the price calculate the amount in each
            // Ex if price is 10 and price per kilo is 20 then there must be 500g in there. Apply this logic to other compare units as well
            return pricePer / pricePerUnit;
        }


        private (string, float) FindCompareUnitAndPrice(string unprocessedDescription, float price)
        {
            unprocessedDescription = unprocessedDescription.ToLowerInvariant();

            // Normalize common delimiters
            unprocessedDescription = unprocessedDescription.Replace(",", "."); // Decimal comma

            // Weight-based units (convert to KG)
            if (IsWeightUnit(unprocessedDescription))
            {
                return HandleWeightUnits(unprocessedDescription, price);
            }

            // Volume-based units (convert to LITER)  
            if (IsVolumeUnit(unprocessedDescription))
            {
                return HandleVolumeUnits(unprocessedDescription, price);
            }

            // Everything else becomes STK
            return HandleCountUnits(unprocessedDescription, price);
        }

        private bool IsWeightUnit(string description)
        {
            // Check for weight-related patterns
            return Regex.IsMatch(description, @"pr\.?\s*(kg|kilo|kilogram|1/2\s*kg|½\s*kg|100\s*g|gr|gram)", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(description, @"/(kg|kilo|kilogram|gr|gram)$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(description, @"\b(kg|kilo|kilogram|gr|gram)\b", RegexOptions.IgnoreCase);
        }

        private bool IsVolumeUnit(string description)
        {
            // Check for volume-related patterns  
            return Regex.IsMatch(description, @"pr\.?\s*(liter|litr|ltr|l|cl|ml|centiliter|milliliter)", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(description, @"/(liter|litr|ltr|l|cl|ml)$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(description, @"\b(liter|litr|ltr|l|cl|ml|centiliter|milliliter)\b", RegexOptions.IgnoreCase);
        }

        private (string, float) HandleWeightUnits(string description, float price)
        {
            // Handle specific weight patterns
            if (Regex.IsMatch(description, @"pr\.?\s*1/2\s*kg", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(description, @"pr\.?\s*½\s*kg", RegexOptions.IgnoreCase))
            {
                return ("kg", ExtractPricePerUnit(description) * 2); // Price per kg
            }

            if (Regex.IsMatch(description, @"pr\.?\s*kg", RegexOptions.IgnoreCase))
            {
                return ("kg", ExtractPricePerUnit(description));
            }

            if (Regex.IsMatch(description, @"pr\.?\s*100\s*g", RegexOptions.IgnoreCase))
            {
                return ("kg", ExtractPricePerUnit(description) * 10); // Convert 100g price to kg price
            }

            // Default weight fallback - assume it's per piece if no clear weight unit
            return ("stk", price);
        }

        private (string, float) HandleVolumeUnits(string description, float price)
        {
            // Handle specific volume patterns
            if (Regex.IsMatch(description, @"pr\.?\s*(liter|litr|ltr|l)\b", RegexOptions.IgnoreCase))
            {
                return ("liter", ExtractPricePerUnit(description));
            }

            if (Regex.IsMatch(description, @"pr\.?\s*cl", RegexOptions.IgnoreCase))
            {
                return ("liter", ExtractPricePerUnit(description) * 100); // Convert cl to liter
            }

            if (Regex.IsMatch(description, @"pr\.?\s*ml", RegexOptions.IgnoreCase))
            {
                return ("liter", ExtractPricePerUnit(description) * 1000); // Convert ml to liter
            }

            // Default volume fallback
            return ("stk", price);
        }

        private (string, float) HandleCountUnits(string description, float price)
        {
            // Handle specific count patterns first
            if (Regex.IsMatch(description, @"^/(stk|pk|bk|sæt|par|styk|piece|pieces)$", RegexOptions.IgnoreCase))
            {
                return ("stk", price);
            }

            // Handle /number patterns (like /12 means price for 12 pieces)
            if (Regex.IsMatch(description, @"^/(\d+)"))
            {
                Match m = Regex.Match(description, @"^/(\d+)");
                if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float count))
                {
                    return ("stk", price / count); // Price per single piece
                }
            }

            // Handle "pr. stk" variations
            if (Regex.IsMatch(description, @"pr\.?\s*(stk|styk|piece|pk|sæt|par)", RegexOptions.IgnoreCase))
            {
                // Special case: if it says "pk stk pris" or similar, return 1 stk
                if (Regex.IsMatch(description, @"(pk|sæt|par).*stk.*pris", RegexOptions.IgnoreCase))
                {
                    return ("stk", price); // This ensures 1 stk instead of trying to extract price
                }

                return ("stk", price);
            }

            // Default fallback for everything else (boxes, sets, packages, etc.)
            return ("stk", price);
        }


        public float ExtractPricePerUnit(string input)
        {
            var match = Regex.Match(input, @"(\d+([.,]\d+)?)\s*$");
            if (!match.Success)
                throw new FormatException("No valid price found at end of string");

            // Use invariant culture to handle both comma and dot as decimal separators
            string priceText = match.Groups[1].Value.Replace(",", ".");
            return float.Parse(priceText, CultureInfo.InvariantCulture);
        }

    }
}