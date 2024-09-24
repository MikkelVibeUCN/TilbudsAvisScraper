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
        private int counter = 0;
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
                    reachedEnd = true;
                }
            }
            return products;
        }

        private float GetPriceOfProduct(string productHtml)
        {
            try
            {
                return GetInformationFromHtml<float>(productHtml, "price-normal-discount\"", ">", "<");

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

        private dynamic GetInformationFromHtml<T>(string productHtml, string searchPattern, string startSearchKey, string endSearchKey)
        {
            int startIndex = productHtml.IndexOf(searchPattern);
            if (startIndex != -1 && productHtml.Contains(startSearchKey))
            {
                startIndex = productHtml.IndexOf(startSearchKey, startIndex) + startSearchKey.Length; // Move past the startSearchKey
                int endIndex = productHtml.IndexOf(endSearchKey, startIndex); // Find the closing tag
                string information = productHtml.Substring(startIndex, endIndex - startIndex).Trim();
                if (typeof(T) == typeof(float)) { return float.Parse(information); }
                else if (typeof(T) == typeof(int)) { return int.Parse(information); }
                else if (typeof(T) == typeof(string)) { return information; }
                else throw new InvalidCastException($"{typeof(T).FullName} is not supported");
            } 
            throw new KeyNotFoundException($"searchpattern: {searchPattern} wasn't found");
        }
    }
}