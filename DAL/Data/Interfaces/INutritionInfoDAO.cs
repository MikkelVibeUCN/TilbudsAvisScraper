using DAL.Data.Batch;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface INutritionInfoDAO
    {
        Task<List<Product>> AddNutritionInfosInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction);

        Task AddNutritionInfo(NutritionInfo nutritionInfo, int productId, SqlConnection connection, SqlTransaction transaction);

        Task<NutritionInfo> GetNutritionForProduct(int productId);

    }
}
