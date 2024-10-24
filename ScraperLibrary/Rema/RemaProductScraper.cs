﻿using ScraperLibrary.Interfaces;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http;
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

        public async Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId)
        {
            string result = await CallUrl(_remaProductPageUrl);
            int lengthOfResult = result.Length;
            List<Product> products = new List<Product>();

            int currentIndex = 0;
            bool reachedEnd = false;
            while (!reachedEnd)
            {
                if(token.IsCancellationRequested)
                {
                    Debug.WriteLine("Cancel requested");

                    token.ThrowIfCancellationRequested();
                }

                string startPattern = "product-grid-container\"";
                string endPattern = "class=\"add-mobile-btn\"";

                int startIndex = result.IndexOf(startPattern, currentIndex);
                int endIndex = int.MaxValue;

                progressCallback((int)(((double)startIndex / lengthOfResult) * 100));

                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    endIndex = result.IndexOf(endPattern, startIndex);
                    // Adjust startIndex to skip the pattern itself
                    startIndex += startPattern.Length;

                    // Extract the product from the html
                      Product product = await CreateProduct(result, startIndex, endIndex, avisExternalId);

                    if (product == null)
                    {
                        Debug.WriteLine("Failed to create product");
                    }
                    else
                    {
                        products.Add(product);
                        Debug.WriteLine(product.ToString());
                    }

                    currentIndex = endIndex + endPattern.Length;


                    //// HARDCODED STOP FOR TESTING REMOVE FOR PROD
                    //if(products.Count > 15)
                    //{
                    //    reachedEnd = true;
                    //}
                }
                else
                {
                    reachedEnd = true;
                }
            }
            return products;
        }

        private async Task<Product> CreateProduct(string result, int startIndex, int endIndex, string avisExternalId)
        {
            string productHtml = result.Substring(startIndex, endIndex - startIndex);

            string externalProductId = GetExternalProductId(productHtml);

            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    var productJson = await GetProductJson(externalProductId);
                    string description = GetDescriptionOfProduct(productJson);
                    List<Price> prices = GetPricesOfProduct(productJson, avisExternalId);
                    string[] units = { "GR.", "STK.", "KG.", "ML.", "CL.", "LTR.", "BAKKE" };

                    var firstPart = description.Split('/')[0].Replace(" ", string.Empty);

                    string unitOfMeasurement = "";

                    foreach (string unit in units)
                    {
                        if (firstPart.Contains(unit))
                        {
                            unitOfMeasurement = unit;
                        }
                        firstPart = firstPart.Replace(unit, "");

                    }
                    float amountInProduct = (float)Math.Round(float.Parse(firstPart), 3);

                    return new Product(prices,
                        null,
                        GetNameOfProduct(productJson),
                        GetProductUrlFromHtml(productHtml),
                        description,
                        externalProductId,
                        GetNutritionalInfo(productJson),    
                        IProductScraper.GetAmountInProduct(amountInProduct, prices, unitOfMeasurement)
                        );
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    retryCount++;
                    Debug.WriteLine("Failed");
                    await Task.Delay(7500);
                    Debug.WriteLine("Retrying");
                }
            }
            Debug.WriteLine("Gave up too many attempts");
            return null;

        }

        private string GetProductUrlFromHtml(string productHtml)
        {
            try { return GetInformationFromHtml<string>(productHtml, "product-grid-image", "src=\"", "\""); } catch { return ""; }
        }

        private string GetExternalProductId(string productHtml)
        {
            try { return GetInformationFromHtml<string>(productHtml, "product-grid-image", "https://cphapp.rema1000.dk/api/v1/catalog/store/1/item/", "/"); } catch { return ""; }
        }

    }
}