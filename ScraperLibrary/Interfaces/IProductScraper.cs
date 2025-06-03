using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Interfaces
{
    public interface IProductScraper
    {
        Task<List<ProductDTO>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId, int companyId, dynamic JSON = null);

        public static float GetAmountInProduct(float amountInProduct, string productInUnit, List<PriceDTO> pricesAssosiated)
        {
            string[] possibleCompareUnitsForAmount = { "kg", "ltr", "stk", "bakke" };
            foreach (PriceDTO price in pricesAssosiated)
            {
                switch (price.CompareUnit)
                {
                    case "kg":
                        if (productInUnit.Equals("g"))
                        {
                            return amountInProduct / 1000;
                        }
                        return amountInProduct;
                    case "ltr":
                        if (productInUnit.Equals("ml"))
                        {
                            return amountInProduct / 1000;
                        }
                        else if (productInUnit.Equals("cl"))
                        {
                            return amountInProduct / 100;
                        }
                        return amountInProduct;
                    case "stk":
                    case "bakke":
                    case "pk":
                        return amountInProduct;
                    default:
                        throw new Exception(price.CompareUnit + " productinunit: " + productInUnit + " is not a valid unit");
                }
            }
            throw new Exception("No prices");
        }
    }
}
