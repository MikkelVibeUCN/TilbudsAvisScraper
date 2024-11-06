using System.Collections;
using System.Text.Json.Serialization;

namespace TilbudsAvisLibrary.Entities
{
    public class Avis
    {
        public List<Page> Pages { get; set; }
        public List<Product> Products { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int Id { get; private set; }
        public string ExternalId { get; set; }

        [JsonConstructor]
        public Avis(string externalId, DateTime validFrom, DateTime validTo, List<Product> products)
        {
            ExternalId = externalId;
            ValidFrom = validFrom;
            ValidTo = validTo;
            Products = new List<Product>(products);
        }

        public Avis(string externalId, DateTime validFrom, DateTime validTo, List<Page> pages, List<Product> products)
        {
            ExternalId = externalId;
            ValidFrom = validFrom;
            ValidTo = validTo;
            Pages = new List<Page>(pages);
            Products = new List<Product>(products);
        }

        public Avis(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
            Products = new List<Product>();
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