using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisLibrary;
using TilbudsAvisLibrary.DTO;

namespace APIIntegrationLibrary
{
    public interface IProductRestClient 
    {
        Task<List<ProductDTO>> GetProducts(ProductQueryParameters parameters);
    }
}
