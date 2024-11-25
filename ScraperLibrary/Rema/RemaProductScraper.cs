using TilbudsAvisLibrary.DTO;
using ScraperLibrary.Interfaces;
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

        public async Task<List<ProductDTO>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId)
        {
            string result = await CallUrl(_remaProductPageUrl);
            int lengthOfResult = result.Length;
            List<ProductDTO> products = new List<ProductDTO>();

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
                    ProductDTO product = await CreateProduct(result, startIndex, endIndex, avisExternalId, companyId);

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

        private async Task<ProductDTO?> CreateProduct(string result, int startIndex, int endIndex, string avisExternalId, int companyId)
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
                    List<PriceDTO> prices = GetPricesOfProduct(productJson, avisExternalId);
                    string[] remaUnits = { "GR.", "STK.", "KG.", "ML.", "CL.", "LTR.", "BAKKE", "PK." };

                    string[] standardUnits = ConvertUnitsToStandard(remaUnits);


                    var firstPart = description.Split('/')[0].Replace(" ", string.Empty);

                    string unitOfMeasurement = "";

                    foreach (string unit in remaUnits)
                    {
                        if (firstPart.Contains(unit))
                        {
                            unitOfMeasurement = standardUnits[Array.IndexOf(remaUnits, unit)];
                        }
                        firstPart = firstPart.Replace(unit, "");
                    }
                    float amountInProduct = (float)Math.Round(float.Parse(firstPart), 3);

                    return new ProductDTO
                    {
                        Prices = prices,
                        Name = GetNameOfProduct(productJson),
                        ImageUrl = GetProductUrlFromHtml(productHtml),
                        Description = description,
                        ExternalId = externalProductId,
                        NutritionInfo = GetNutritionalInfo(productJson),
                        Amount = IProductScraper.GetAmountInProduct(amountInProduct, unitOfMeasurement, prices),
                    };
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