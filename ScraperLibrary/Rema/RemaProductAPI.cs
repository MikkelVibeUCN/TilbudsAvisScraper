using APIIntegrationLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Rema
{
    public abstract class RemaProductAPI : Scraper
    {
        public static NutritionInfoDTO? GetNutritionalInfo(dynamic jsonResponse)
        {
            float energyKJ = 0, fat = 0, saturatedFat = 0, carbohydrates = 0, sugars = 0, fiber = 0, protein = 0, salt = 0;

            var nutritionInfoArray = jsonResponse.GetProperty("data").GetProperty("nutrition_info").EnumerateArray();

            foreach (var item in nutritionInfoArray)
            {
                string name = item.GetProperty("name").GetString();
                string value = item.GetProperty("value").GetString().Trim().Replace(",", ".");
                if (value.StartsWith('<') || value.StartsWith('>'))
                {
                    value = value.Substring(2);
                }
                value = value.Replace("Ca. ", ""); // Remove "Ca." from the value string  

                switch (name)
                {
                    case "Energi":
                        value = value.Replace(".", string.Empty);
                        energyKJ = float.Parse(value.Split(' ')[0]);
                        break;
                    case "Fedt":
                        fat = float.Parse(value);
                        break;
                    case "Heraf mættede fedtsyrer":
                        saturatedFat = float.Parse(value);
                        break;
                    case "Kulhydrat":
                        carbohydrates = float.Parse(value);
                        break;
                    case "Heraf sukkerarter":
                        sugars = float.Parse(value);
                        break;
                    case "Kostfibre":
                        fiber = float.Parse(value);
                        break;
                    case "Protein":
                        protein = float.Parse(value);
                        break;
                    case "Salt":
                        salt = float.Parse(value);
                        break;
                }
            }
            if (energyKJ == 0f && fat == 0f && saturatedFat == 0f && carbohydrates == 0f && sugars == 0f && fiber == 0f && salt == 0f)
            {
                return null;
            }
            return new NutritionInfoDTO
            {
                EnergyKJ = energyKJ,
                FatPer100G = fat,
                CarbohydratesPer100G = carbohydrates,
                FiberPer100G = fiber,
                ProteinPer100G = protein,
                SaltPer100G = salt,
                SugarsPer100G = sugars,
            };
        }

        

        public static string GetNameOfProduct(dynamic jsonResponse)
        {
            return jsonResponse.GetProperty("data").GetProperty("name").GetString();
        }

        public static string GetDescriptionOfProduct(dynamic jsonResponse)
        {
            return jsonResponse.GetProperty("data").GetProperty("underline").GetString();
        }

        public static List<PriceDTO> GetPricesOfProduct(dynamic jsonResponse, string avisExternalId)
        {
            List<PriceDTO> prices = new List<PriceDTO>();

            float priceValue = 0;
            string compareUnitString = "";

            var priceArray = jsonResponse.GetProperty("data").GetProperty("prices").EnumerateArray();

            foreach (var priceItem in priceArray)
            {
                priceValue = priceItem.GetProperty("price").GetSingle();
                compareUnitString = priceItem.GetProperty("compare_unit").GetString();

                bool basePrice = !priceItem.GetProperty("is_advertised").GetBoolean();

                if (basePrice)
                {
                    prices.Add(new PriceDTO
                    {
                        Price = priceValue,
                        CompareUnit = compareUnitString,
                        ExternalAvisId = "base"
                    });
                }
                else
                {
                    prices.Add(new PriceDTO
                    {
                        Price = priceValue,
                        CompareUnit = compareUnitString,
                        ExternalAvisId = avisExternalId
                    });
                }
            }
            return prices;
        }

        public async Task<dynamic> GetProductJson(string externalId)
        {
            await Task.Delay(new Random().Next(200, 300));

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("https://cphapp.rema1000.dk/api/v3/products/" + externalId + "?include=declaration,nutrition_info,declaration,warnings");

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<dynamic>(await response.Content.ReadAsStreamAsync());
            }
            throw new Exception("Failed to get product json");
        }
    }
}
