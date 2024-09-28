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

                    // Extract the product from the html
                    Product product = CreateProductFromHtml(result, startIndex, endIndex);

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

        private Product CreateProductFromHtml(string result, int startIndex, int endIndex)
        {
            string productHtml = result.Substring(startIndex, endIndex - startIndex);

            return new(GetNameOfProduct(productHtml),
                GetProductUrlFromHtml(productHtml),
                GetDescriptionOfProduct(productHtml),
                GetExternalProductId(productHtml),
                [new Price(GetPriceOfProduct<float>(productHtml))]
                );
        }

        private dynamic GetPriceOfProduct<T>(string productHtml)
        {
            return GetInformationFromHtml<T>(productHtml, "price-normal-discount\"", ">", "<");
        }

        private string GetNameOfProduct(string productHtml)
        {
            return GetInformationFromHtml<string>(productHtml, "class=\"title\"", ">", "<");
        }

        private string GetProductUrlFromHtml(string productHtml)
        {
            return GetInformationFromHtml<string>(productHtml, "product-grid-image", "src=\"", "\"");
        }

        private string GetDescriptionOfProduct(string productHtml)
        {
            return GetInformationFromHtml<string>(productHtml, "extra", "\"\">", "<");
        }

        private int GetExternalProductId(string productHtml)
        {
            return GetInformationFromHtml<int>(productHtml, "product-grid-image", "https://cphapp.rema1000.dk/api/v1/catalog/store/1/item/", "/");
        }
    }
}