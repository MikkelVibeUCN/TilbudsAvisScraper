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

                ProductDTO newProduct = CreateProduct(productData, avisExternalId);
                products.Add(newProduct);

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
            string imageUrl = productJSON.image;

            float price = productJSON.price;

            // Find compare unit and price per said unit
            string unprocessedDescription = productJSON.basicPrice;

            (string compareUnit, float pricePerUnit) = FindCompareUnitAndPrice(unprocessedDescription, price);

            // Calculate the amount based on the price and compare unit
            float amount = CalculateAmount(price, pricePerUnit);

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
                string strokePriceText = (string) strokePrice.Value;

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
                _ => throw new ArgumentException($"Unknown compare unit: {compareUnit}", nameof(compareUnit))
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
                    : rounded.ToString("0.##");
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
            string[] possibleSearchIdentifies = { "Pr. kg", "Pr. liter", "Pr. stk" };

            // Clauses to catch edgecases
            switch (unprocessedDescription)
            {
                case "/stk":
                case "/pk":
                case "/bk":
                case "/sæt":
                case "/par":
                case "/sæt./stk":
                case "/stk./pk":
                    return ("stk", price);
            }

            // Check if description contains a "Pr. [unit]" pattern first
            foreach (string possibleUnit in possibleSearchIdentifies)
            {
                if (unprocessedDescription.Contains(possibleUnit, StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the unit after "Pr. "
                    string unit = possibleUnit.Substring(3).Trim();
                    float pricePerUnit = ExtractPricePerUnit(unprocessedDescription);
                    return (unit, pricePerUnit);
                }
            }

            // Specific instance where it starts with / and is followed by a number
            if (Regex.IsMatch(unprocessedDescription, @"^/\d+"))
            {
                // Extract the number after the slash
                Match match = Regex.Match(unprocessedDescription, @"^/(\d+)");
                if (match.Success)
                {
                    float quantity = float.Parse(match.Groups[1].Value, locale);
                    return ("stk", price / quantity);
                }
            }

            throw new Exception("Compare unit not found in description: " + unprocessedDescription);
        }

        public float ExtractPricePerUnit(string input)
        {
            // Step 1: Reverse the string
            string reversed = new string(input.Reverse().ToArray());

            // Step 2: Find the index of the first space
            int spaceIndex = reversed.IndexOf(' ');

            // Step 3: Take substring up to that space
            string reversedWord = spaceIndex >= 0 ? reversed.Substring(0, spaceIndex) : reversed;

            // Step 4: Reverse it back
            string finalWord = new string(reversedWord.Reverse().ToArray());

            return float.Parse(finalWord, locale);
        }
    }
}