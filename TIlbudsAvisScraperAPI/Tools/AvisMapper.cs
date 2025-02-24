using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Tools
{
    public static class AvisMapper
    {
        public static AvisDTO MapAvisToDTO(Avis avis, List<ProductDTO> products)
        {
            return new AvisDTO()
            {
                Products = products,
                ExternalId = avis.ExternalId,
                Id = avis.Id,
                ValidFrom = avis.ValidFrom,
                ValidTo = avis.ValidTo
            };
        }
    }
}
