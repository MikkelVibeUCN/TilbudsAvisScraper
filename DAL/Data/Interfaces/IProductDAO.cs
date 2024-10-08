using DAL.Data.DataTransferObject;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface IProductDAO : IDAO<Product>
    {
        Task<List<Product>> AddProducts(List<Product> products, SqlTransaction transaction, SqlConnection connection, int baseAvisId, int avisId, string externalAvisId);
        Task<List<Product>> AddProducts(List<Product> products, int baseAvisId, int avisId, string externalAvisId);
        Task<bool> DeleteOnExternalId(int externalId);
        Task<bool> DeleteNegativeExternalIds();
    }
}