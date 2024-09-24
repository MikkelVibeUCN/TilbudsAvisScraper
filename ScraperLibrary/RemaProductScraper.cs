using ScraperLibrary.Interfaces;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary
{
    public class RemaProductScraper : Scraper, IProductScraper
    {
        public RemaProductScraper()
        {

        }

        public async Task<List<Product>> GetAllProductsFromPage(string url)
        {
            string result = await Scraper.CallUrl(url);
            List<Product> products = [];
            
            int currentIndex = 0;
            bool reachedEnd = false;
            while (!reachedEnd)
            {
                string startPattern = "product-grid-container\"";
                string endPattern = "class=\"add-mobile-btn\"";

                int startIndex = result.IndexOf(startPattern, currentIndex);
                int endIndex = int.MaxValue;

                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    endIndex = result.IndexOf(endPattern, startIndex);
                    // Adjust startIndex to skip the pattern itself
                    startIndex += startPattern.Length;

                    // Extract the product HTML between the two patterns
                    string productHtml = result.Substring(startIndex, endIndex - startIndex);

                    Product product = new(GetNameOfProduct(productHtml), 
                        GetPriceOfProduct<float>(productHtml), 
                        GetProductUrlFromHtml(productHtml), 
                        GetDescriptionOfProduct(productHtml),
                        GetExternalProductId(productHtml),
                        null
                        );
                    products.Add(product);
                    Console.WriteLine(product.ToString());
                    currentIndex = endIndex + endPattern.Length;
                }
                else
                {
                    reachedEnd = true;
                }
            }
            return products;
        }

        private dynamic GetPriceOfProduct<T>(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<T>(productHtml, "price-normal-discount\"", ">", "<");

            }
            catch 
            {
                return -1;
            }
        }

        private string GetNameOfProduct(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<string>(productHtml, "class=\"title\"", ">", "<");
            }
            catch
            {
                return "";
            }
        }

        private string GetProductUrlFromHtml(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<string>(productHtml, "product-grid-image", "src=\"", "\"");
            }
            catch
            {
                return "";
            }
        }

        private string GetDescriptionOfProduct(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<string>(productHtml, "extra", "\"\">", "<");
            }
            catch
            {
                return "";
            }
        }

        private int GetExternalProductId(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<int>(productHtml, "product-grid-image", "https://cphapp.rema1000.dk/api/v1/catalog/store/1/item/", "/");
            }
            catch
            {
                return -1;
            }
        }
    }
}