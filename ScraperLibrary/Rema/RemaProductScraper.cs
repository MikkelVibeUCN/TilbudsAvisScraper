using ScraperLibrary.Interfaces;
using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Rema
{
    public class RemaProductScraper : RemaProductAPI, IProductScraper
    {
        private const string _remaProductPageUrl = "https://shop.rema1000.dk/avisvarer";

        public RemaProductScraper()
        {

        }

        public async Task<List<Product>> GetAllProductsFromPage()
        {
            string result = await CallUrl(_remaProductPageUrl);
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

                    if (product == null)
                    {
                        Console.WriteLine("Failed to create product");
                    }
                    else
                    {
                        products.Add(product);
                        Console.WriteLine(product.ToString());
                    }

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

            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    var productJson = await GetProductJson(externalProductId);
                    List<Price> prices = GetPricesOfProduct(productJson);

                    return new Product(prices,
                        null,
                        GetNameOfProduct(productJson),
                        GetProductUrlFromHtml(productHtml),
                        GetDescriptionOfProduct(productJson),
                        externalProductId,
                        GetNutritionalInfo(productJson),
                        GetAmountInProduct(productJson, prices)
                        );
                }
                catch (Exception e)
                {
                    retryCount++;
                    Console.WriteLine("Failed");
                    await Task.Delay(7500);
                    Console.WriteLine("Retrying");
                }
            }
            Console.WriteLine("Gave up too many attempts");
            return null;

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