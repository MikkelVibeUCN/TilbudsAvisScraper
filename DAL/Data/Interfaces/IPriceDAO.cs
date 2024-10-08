using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface IPriceDAO : IDAO<Price>
    {
        Task AddPricesForProduct(SqlConnection connection, SqlTransaction transaction, Product product, int baseAvisId, int avisId, string avisExternalId);
        Task AddPricesForProduct(Product product, int baseAvisId, int avisID, string avisExternalId);
        Task AddPricesForProducts(SqlConnection connection, SqlTransaction transaction, List<Product> products, int baseAvisId, int avisId, string avisExternalId);
        Task AddPricesForProducts(List<Product> products, int baseAvisId, int avisId, string avisExternalId);
        Task<List<Price>> GetPricesForProduct(int productId);
    }
}