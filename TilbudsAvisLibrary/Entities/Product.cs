using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TilbudsAvisLibrary.Entities
{
    public class Product : IParameters
    {
        public List<Price> Prices { get; set; }
        public int? Id { get; private set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public float? Amount { get; set; }  
        public NutritionInfo? NutritionInfo { get; set; }

        [JsonConstructor]
        public Product(List<Price> prices, int? id, string name, string imageUrl, string description, string externalId, NutritionInfo? nutritionInfo, float? amount)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            Prices = prices;
            NutritionInfo = nutritionInfo;
            Amount = amount;
        }

        public Product(List<Price> prices, string name, string imageUrl, string description, string externalId, float amount, NutritionInfo? nutritionInfo = null, int? id = null)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            Prices = prices;
            NutritionInfo = nutritionInfo;
            Amount = amount;
        }

        public void CalculateUnitPrice()
        {
            foreach (var price in Prices)
            {
                price.UnitPrice = price.PriceValue / (float)Amount;
            }
        }

        public void SetId(int id) => Id = id;

        public override string ToString()
        {
            return $"Name: {Name}, ImageUrl: {ImageUrl}, Description: {Description}, ExternalId: {ExternalId}";
        }

        public void AddPrice(Price price)
        {
            Prices.Add(price);
        }

        public IEnumerable<Price> GetPrices()
        {
            return new List<Price>(Prices);
        }

        public void RemovePrice(Price price)
        {
            Prices.Remove(price);
        }

        public int TotalParameterAmount()
        {
            int amount = 5;
            if(Prices.Count > 0)
            {
                amount += Prices.Count * Prices[0].TotalParameterAmount();
            }
            if (NutritionInfo != null)
            {
                amount += NutritionInfo.TotalParameterAmount();
            }
            return amount;
        }
    }
}