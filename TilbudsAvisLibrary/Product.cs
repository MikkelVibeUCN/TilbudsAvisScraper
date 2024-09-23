using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary
{
    public class Product
    {
        int? id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int ExternalId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        public Product(string name, float price, string imageUrl, string description, int externalId, int? id)
        {
            this.Name = name;
            this.Price = price;
            this.ImageUrl = imageUrl;
            this.Description = description;
            this.ExternalId = externalId;
            this.id = id;
        }

        public override string ToString() 
        {
            return $"Name: {Name}, Price: {Price}, ImageUrl: {ImageUrl}, Description: {Description}, ExternalId: {ExternalId}";
        }
    }
}