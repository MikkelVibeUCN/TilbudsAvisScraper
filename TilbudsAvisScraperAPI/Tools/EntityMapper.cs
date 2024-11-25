using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Tools
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
    }
}
