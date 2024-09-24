namespace TilbudsAvisLibrary
{
    public class Page
    {
        public string ImageUrl { get; set; }
        private List<Product> Products = new List<Product>();
        public Page(string imageUrl)
        {
            this.ImageUrl = imageUrl;
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
