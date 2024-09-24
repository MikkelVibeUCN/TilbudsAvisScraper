namespace TilbudsAvisLibrary.Entities
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
            Name = name;
            Price = price;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
            this.id = id;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Price: {Price}, ImageUrl: {ImageUrl}, Description: {Description}, ExternalId: {ExternalId}";
        }
    }
}