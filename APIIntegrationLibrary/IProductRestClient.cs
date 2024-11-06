using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisLibrary;

namespace APIIntegrationLibrary
{
    public interface IProductRestClient 
    {
        Task<List<Product>> Get10BestProducts();
        Task<Product> GetProductById(int id);
        Task<List<Product>> GetProducts(ProductQueryParameters parameters);
    }
}
