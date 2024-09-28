using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface IProductDAO : IDAO<Product>
    {
        Task<List<Product>> AddProducts(IEnumerable<Product> products, int avisId);
    }
}