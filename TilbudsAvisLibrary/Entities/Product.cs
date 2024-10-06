using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TilbudsAvisLibrary.Entities
{
    public class Product
    {
        public List<Price> Prices { get; set; }
        public int? Id { get; private set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        
        public string Description { get; set; }
        public int ExternalId { get; set; }
        public float Amount { get; set; } 
        public NutritionInfo NutritionInfo { get; set; }

        [JsonConstructor]
        public Product(List<Price> prices, int? id, string name, string imageUrl, string description, int externalId, NutritionInfo nutritionInfo, float amount)
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

        public Product(string name, string imageUrl, string description, int externalId, int? id, List<Price> prices, NutritionInfo nutritionInfo, float amount)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            Prices = prices ?? new List<Price>();
            NutritionInfo = nutritionInfo;
            Amount = amount;
        }

        public void CalculateUnitPrice()
        {
            foreach (var price in Prices)
            {
                price.UnitPrice = price.PriceValue / Amount;
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
    }
}