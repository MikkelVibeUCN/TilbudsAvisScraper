﻿using System.Text.Json.Serialization;

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
        public NutritionInfo _NutritionInfo { get; set; }

        [JsonConstructor]
        public Product(string name, string imageUrl, string description, int externalId, List<Price> prices, NutritionInfo nutritionInfo)
        {
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            Prices = prices ?? new List<Price>();
            _NutritionInfo = nutritionInfo;
        }

        public Product(string name, string imageUrl, string description, int externalId, int? id, List<Price> prices)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            Prices = prices ?? new List<Price>();
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