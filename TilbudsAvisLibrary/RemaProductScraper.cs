using Emgu.CV.Stitching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Interfaces;

namespace TilbudsAvisLibrary
{
    public class RemaProductScraper : Scraper, IProductScraper
    {
        public RemaProductScraper()
        {

        }

        public async Task<List<Product>> GetAllProductsFromPage(string url)
        {
            string result = await Scraper.CallUrl(url);
            List<Product> products = new List<Product>();
            
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

                    Product product = new Product(GetNameOfProduct(productHtml), 
                        GetPriceOfProduct(productHtml), 
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
                    // If no more products are found, end the loop
                    reachedEnd = true;
                }
            }

            return products;
        }

        private float GetPriceOfProduct(string productHtml)
        {
            return GetInformationFromHtml<float>(productHtml, "price-normal-discount\"", ">", "<");
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

        private dynamic GetInformationFromHtml<T>(string productHtml, string searchPattern, string startSearchKey, string endSearchKey)
        {
            int startIndex = productHtml.IndexOf(searchPattern);
            if (startIndex != -1)
            {
                startIndex = productHtml.IndexOf(startSearchKey, startIndex) + startSearchKey.Length; // Move past the startSearchKey
                int endIndex = productHtml.IndexOf(endSearchKey, startIndex); // Find the closing tag
                string information = productHtml.Substring(startIndex, endIndex - startIndex).Trim();
                if (typeof(T) == typeof(float)) { return float.Parse(information); }
                else if (typeof(T) == typeof(int)) { return int.Parse(information); }
                else if (typeof(T) == typeof(string)) { return information; }
                else return new InvalidCastException($"{typeof(T).FullName} is not supported");
            } 
            return new KeyNotFoundException($"searchpattern: {searchPattern} wasn't found");
        }
    }
}
