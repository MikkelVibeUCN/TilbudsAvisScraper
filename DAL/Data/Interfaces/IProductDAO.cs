using DAL.Data.Batch;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TilbudsAvisLibrary;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface IProductDAO
    {
        Task<List<Product>> AddProducts(List<Product> products, SqlTransaction transaction, SqlConnection connection, int baseAvisId, int avisId, string externalAvisId, int companyId);
        Task<List<Product>> AddProducts(List<Product> products, int baseAvisId, int avisId, string externalAvisId, int companyId);
        Task<bool> DeleteOnExternalId(int externalId);
        Task<bool> DeleteTestProducts();
        Task<int> Add(Product product, int baseAvisId, int avisId, string avisBaseExternalId, int companyId);
        Task<List<Company>> GetAllProdudctsWithInformationFromCompany(ProductQueryParameters parameters);
    }
}