using DAL.Data.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.Data.DAO.ProductDAO;

namespace DAL.Data.DAO
{
    public abstract class DAObject
    {
        protected string ConnectionString;
        private readonly int maxBatchSize = 100;

        public DAObject()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(userProfile, "source", "repos", "TilbudsAvisScraper", "databaselogin.txt");

            ConnectionString = File.ReadAllText(filePath);
        }
        protected string GetAllFromTableQueryOnCondition(string tableName, string identifer)
        {
            return $"SELECT * FROM {tableName} where {identifer} = @{identifer};";
        }

        protected string DeleteFromTableQueryWhereCondition(string tableName, string identifier)
        {
            return $"DELETE FROM {tableName} WHERE {identifier} = @{identifier};";
        }

        protected async Task<List<T>> AddTInBatch<T>(
                 List<T> batch,
                 SqlConnection connection,
                 SqlTransaction transaction,
                 Func<List<T>, SqlConnection, SqlTransaction, BatchContext, Task<List<T>>> addBatchMethod,
                 BatchContext? context = null) // Default value to null
        {
            List<T> result = new List<T>();

            int totalInBatch = batch.Count;

            for (int batchStart = 0; batchStart < totalInBatch; batchStart += maxBatchSize)
            {
                var batchOfT = batch.Skip(batchStart).Take(maxBatchSize).ToList();
                result.AddRange(await addBatchMethod(batchOfT, connection, transaction, context));
            }
            return result;
        }
    }
}