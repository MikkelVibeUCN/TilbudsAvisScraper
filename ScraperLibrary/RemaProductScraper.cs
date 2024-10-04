using ScraperLibrary.Interfaces;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary
{
    public class RemaProductScraper : RemaProductAPI, IProductScraper
    {
        private const string _remaProductPageUrl = "https://shop.rema1000.dk/avisvarer";

        public RemaProductScraper()
        {

        }

        public async Task<List<Product>> GetAllProductsFromPage()
        {
            string result = await Scraper.CallUrl(_remaProductPageUrl);
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
                    Product product = await CreateProduct(result, startIndex, endIndex);

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

        private async Task<Product> CreateProduct(string result, int startIndex, int endIndex)
        {
            string productHtml = result.Substring(startIndex, endIndex - startIndex);

            int externalProductId = GetExternalProductId(productHtml);

            var productJson = await GetProductJson(externalProductId);

            return new Product(GetNameOfProduct(productJson),
                GetProductUrlFromHtml(productHtml),
                GetDescriptionOfProduct(productJson),
                externalProductId,
                GetPricesOfProduct(productJson),
                GetNutritionalInfo(productJson)
                );
        }

        private string GetProductUrlFromHtml(string productHtml)
        {
            try { return GetInformationFromHtml<string>(productHtml, "product-grid-image", "src=\"", "\""); } catch { return ""; }
        }

        private int GetExternalProductId(string productHtml)
        {
            try { return GetInformationFromHtml<int>(productHtml, "product-grid-image", "https://cphapp.rema1000.dk/api/v1/catalog/store/1/item/", "/"); } catch { return -1; }
        }

    }  
}