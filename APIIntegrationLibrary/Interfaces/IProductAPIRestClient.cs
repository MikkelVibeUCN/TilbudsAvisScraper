using TilbudsAvisLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary;

namespace APIIntegrationLibrary.Interfaces
{
    public interface IProductAPIRestClient
    {
        Task<int> CreateAsync(ProductDTO entity, string? endpoint = null);
        Task<bool> DeleteAsync(int id, string? endpoint = null);
        Task<ProductDTO?> GetAsync(int? id = null, string ? endpoint = null);
        Task<IEnumerable<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters);
        Task<int> GetProductCountAsync(ProductQueryParameters parameters);
        Task<ProductDTO> EditAsync(ProductDTO product, string? endpoint = null);
        Task<IEnumerable<string>> GetValidCompanyNamesFromProductSearch(ProductQueryParameters parameters);
    }
}
