using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.Interfaces
{
    public interface IProductScraper
    {
        Task<List<Product>> GetAllProductsFromPage(Action<int> progressCallback, CancellationToken token, string avisExternalId);

        public static float GetAmountInProduct(float amountInProduct, string productInUnit, List<Price> pricesAssosiated)
        {
            string[] possibleCompareUnitsForAmount = { "kg", "ltr", "stk", "bakke" };
            foreach (Price price in pricesAssosiated)
            {
                switch (price.CompareUnitString)
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
                        return amountInProduct;
                    default:
                        throw new Exception(productInUnit + " is not a valid unit");
                }
            }
            throw new Exception("No prices");
        }
    }
}
