using System.Text.Json.Serialization;

namespace TilbudsAvisLibrary.Entities
{
    public class Avis
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public List<Product> Products { get; set; } = new List<Product>();
        public string CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Id { get; private set; }

        [JsonConstructor]
        public Avis(string companyName, DateTime startDate, DateTime endDate, List<Product> products)
        {
            CompanyName = companyName;
            StartDate = startDate;
            EndDate = endDate;
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
        public List<Product> GetProducts()
        {
            return new List<Product>(Products);
        }
    }
}