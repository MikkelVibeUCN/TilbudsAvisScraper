using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TilbudsAvisScraperAPI.Tools
{
    public static class ProductDTOMapper
    {
        public static List<ProductDTO> ConvertProductsInCompanyToDTO(Company company)
        {
            return company.Aviser.SelectMany(avis => ConvertAvisProductsToDTO(avis.Products, company.Name, avis.ValidFrom, avis.ValidTo)).ToList();
        }

        public static List<ProductDTO> ConvertAvisProductsToDTO(IEnumerable<Product> productEntities, string companyName, DateTime validFrom, DateTime validTo)
        {
            return productEntities.Select(product => MapToDTO(product, companyName, validFrom, validTo)).ToList();
        }

        public static ProductDTO MapToDTO(Product product, string companyName, DateTime validFrom, DateTime validTo)
        {
            List<PriceDTO> prices = new List<PriceDTO>();
            foreach (var price in product.Prices)
            {
                prices.Add(new PriceDTO
                {
                    Price = price.PriceValue,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    CompareUnit = price.CompareUnitString,
                    CompanyName = companyName,
                });
            }

            return new ProductDTO
            {
                Description = product.Description,
                Id = (int)product.Id,
                ImageUrl = product.ImageUrl,
                Name = product.Name,
                NutritionInfo = product.NutritionInfo != null ? MapToDTO(product.NutritionInfo) : null,
                Prices = prices
            };
        }

        public static List<Product> MapToEntity(IEnumerable<ProductDTO> products, string externalAvisID)
        {
            return products.Select(product => MapToEntity(product, externalAvisID)).ToList();
        }

        public static Product MapToEntity(ProductDTO product, string externalAvisID) => new Product
        {
            Prices = product.Prices.Select(MapToEntity).ToList(),
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            NutritionInfo = product.NutritionInfo != null ? MapToEntity(product.NutritionInfo) : null,
            ExternalId = product.ExternalId,
            Amount = product.Amount
        };

        public static Price MapToEntity(PriceDTO price) => new Price
        {
            CompareUnitString = price.CompareUnit,
            PriceValue = price.Price,
            ExternalAvisId = price.ExternalAvisId,
        };
        public static NutritionInfo MapToEntity(NutritionInfoDTO nutritionInfoDTO) => new NutritionInfo
        {
            CarbohydratesPer100G = nutritionInfoDTO.CarbohydratesPer100G,
            EnergyKJ = nutritionInfoDTO.EnergyKcal,
            FatPer100G = nutritionInfoDTO.FatPer100G,
            FiberPer100G = nutritionInfoDTO.FiberPer100G,
            ProteinPer100G = nutritionInfoDTO.ProteinPer100G,
            SaltPer100G = nutritionInfoDTO.SaltPer100G,
            SaturatedFatPer100G = nutritionInfoDTO.SaturatedFatPer100G,
            SugarsPer100G = nutritionInfoDTO.SugarsPer100G
        };

        public static NutritionInfoDTO MapToDTO(NutritionInfo nutritionInfoDTO) => new NutritionInfoDTO
        {
            CarbohydratesPer100G = nutritionInfoDTO.CarbohydratesPer100G,
            EnergyKcal = NutritionInfo.GetEnergyKcal(nutritionInfoDTO.EnergyKJ),
            FatPer100G = nutritionInfoDTO.FatPer100G,
            FiberPer100G = nutritionInfoDTO.FiberPer100G,
            ProteinPer100G = nutritionInfoDTO.ProteinPer100G,
            SaltPer100G = nutritionInfoDTO.SaltPer100G,
            SaturatedFatPer100G = nutritionInfoDTO.SaturatedFatPer100G,
            SugarsPer100G = nutritionInfoDTO.SugarsPer100G,
            Id = nutritionInfoDTO.Id

        };
    }

}
