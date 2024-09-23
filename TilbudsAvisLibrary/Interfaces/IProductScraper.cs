using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Interfaces
{
    public interface IProductScraper
    {

        Task<List<Product>> GetAllProductsFromPage(string url);



    }
}
