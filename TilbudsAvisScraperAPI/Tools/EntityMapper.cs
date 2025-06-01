using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TilbudsAvisScraperAPI.Tools
{
    public static class EntityMapper
    {
        public static Avis MapToEntity(AvisDTO avis)
        {
            List<Product> products = ProductDTOMapper.MapToEntity(avis.Products, avis.ExternalId);

            return new Avis
            {
                Products = products,
                ValidFrom = avis.ValidFrom,
                ValidTo = avis.ValidTo,
                ExternalId = avis.ExternalId
            };
        }

        public static PriceDTO MapToDTO(Price price, Avis avis)
        {
            return new PriceDTO()
            {
                ExternalAvisId = price.ExternalAvisId,
                Price = price.PriceValue,
                ValidFrom = avis.ValidFrom,
                ValidTo = avis.ValidTo,
            };
        }

        public static List<PriceDTO> MapToDTO(List<Price> prices, Avis avis)
        {
            List<PriceDTO> newPrices = new List<PriceDTO>();
            foreach (Price price in prices)
            {
                newPrices.Add(MapToDTO(price, avis));
            }
            return newPrices;
        }

        public static NutritionInfoDTO MapToDTO(NutritionInfo nutritionInfo)
        {
            return new NutritionInfoDTO()
            {
                Id = nutritionInfo.Id,
                EnergyKcal = NutritionInfo.GetEnergyKcal(nutritionInfo.EnergyKJ),
                FatPer100G = nutritionInfo.FatPer100G,
                SaturatedFatPer100G = nutritionInfo.SaturatedFatPer100G,
                CarbohydratesPer100G = nutritionInfo.CarbohydratesPer100G,
                SugarsPer100G = nutritionInfo.SugarsPer100G,
                FiberPer100G = nutritionInfo.FiberPer100G,
                ProteinPer100G = nutritionInfo.ProteinPer100G,
                SaltPer100G = nutritionInfo.SaltPer100G
            };
        }

        public static ProductDTO MapToDTO(Product product, Avis avis)
        {
            List<PriceDTO> prices = MapToDTO(product.Prices, avis);

            return new ProductDTO()
            {
                Amount = product.Amount ??= 0,
                Description = product.Description,
                ExternalId = product.ExternalId,
                ImageUrl = product.ImageUrl,
                Name = product.Name,
                Prices = prices,
                NutritionInfo = product.NutritionInfo != null ? MapToDTO(product.NutritionInfo) : null,
                Id = product.Id
            };
        }

        public static List<ProductDTO> MapToDTO(List<Product> products, Avis avis)
        {
            List<ProductDTO> newList = new List<ProductDTO>();

            foreach (Product p in products)
            {
                newList.Add(MapToDTO(p, avis));
            }

            return newList;
        }

        public static AvisDTO MapToDTO(Avis avis)
        {
            List<ProductDTO> productDtos = MapToDTO(avis.Products, avis);

            return new AvisDTO()
            {
                ExternalId = avis.ExternalId,
                Products = productDtos,
                ValidFrom = avis.ValidFrom,
                ValidTo = avis.ValidTo,
                Id = avis.Id
            };
        }

        public static ProductDTO? MapCompanyToFullProductDTO(List<Company> companies)
        {
            ProductDTO? product = null;

            foreach (var company in companies)
            {
                var products = ProductDTOMapper.ConvertProductsInCompanyToDTO(company);

                if (product == null && products.Any())
                {
                    var firstProduct = products.First();
                    product = new ProductDTO
                    {
                        Amount = firstProduct.Amount,
                        Description = firstProduct.Description,
                        ExternalId = firstProduct.ExternalId,
                        Id = firstProduct.Id,
                        ImageUrl = firstProduct.ImageUrl,
                        Name = firstProduct.Name,
                        NutritionInfo = firstProduct.NutritionInfo,
                        Prices = new List<PriceDTO>()
                    };
                }

                foreach (var prod in products)
                {
                    product?.Prices.Add(prod.Prices.First());
                }
            }

            return product;
        }
    }
}
