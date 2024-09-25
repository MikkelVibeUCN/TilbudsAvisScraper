namespace TilbudsAvisLibrary.Entities
{
    public class Product
    {
        private List<Price> Prices { get; set; }
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int ExternalId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        public Product(string name, float price, string imageUrl, string description, int externalId, int? id)
        {
            Prices = new List<Price>();
            this.Id = id;
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            ExternalId = externalId;
        }

        public override string ToString()
        {
            return $"Name: {Name}, ImageUrl: {ImageUrl}, Description: {Description}, ExternalId: {ExternalId}";
        }

        public void AddPrice(int id, float price)
        {
            Prices.Add(new Price(id, price));
        }

        public IEnumerable<Price> GetPrices()
        {
            return new List<Price>(Prices);
        }

        public void RemovePrice(int id)
        {
            Price price = Prices.Find(p => p.Id == id);
            if(price != null)
                RemovePrice(price);
        }
        public void RemovePrice(Price price)
        {
            Prices.Remove(price);
        }
    }
}