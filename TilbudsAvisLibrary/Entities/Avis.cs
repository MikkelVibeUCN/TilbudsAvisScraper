using System.Collections;
using System.Text.Json.Serialization;

namespace TilbudsAvisLibrary.Entities
{
    public class Avis
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public List<Product> Products { get; set; } = new List<Product>();
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int Id { get; private set; }
        public int ExternalId { get; set; }

        [JsonConstructor]
        public Avis(int externalId, DateTime validFrom, DateTime validTo, List<Product> products)
        {
            ExternalId = externalId;
            ValidFrom = validFrom;
            ValidTo = validTo;
            Products = products ?? new List<Product>();
        }
        public void SetId(int id) => Id = id;

        public void AddPage(Page page)
        {
            Pages.Add(page);
        }
        public void RemovePage(Page page)
        {
            Pages.Remove(page);
        }
        public void AddProduct(Product product)
        {
            Products.Add(product);
        }
        public void DeleteProduct(Product product)
        {
            Products.Remove(product);
        }
        public IEnumerable<Product> GetProducts()
        {
            return new List<Product>(Products);
        }
    }
}