using DAL.Data.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
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
            BatchContext? context = null)
        {
            List<T> result = new List<T>();

            result.AddRange(await addBatchMethod(batch, connection, transaction, context));

            return result;
        }

        private int CalculateMaxBatchSize(List<IParameters> elements, int batchStart, int totalCount)
        {
            int currentBatchSize = 0;
            int totalParameters = 0;

            for (int i = batchStart; i < totalCount; i++)
            {
                var element = elements[i];

                int totalParametersForProduct = element.TotalParameterAmount();

                if (totalParameters + totalParametersForProduct > 2100)
                {
                    break;
                }

                totalParameters += totalParametersForProduct;
                currentBatchSize++;
            }

            return currentBatchSize;
        }

        protected async Task<List<T>> AddElementsWithMaxBatchSize<T>(List<T> elements, SqlConnection connection, SqlTransaction transaction, BatchContext context, Func<List<T>, SqlConnection, SqlTransaction, BatchContext, Task<List<T>>> addBatchMethod) where T : IParameters
        {
            List<T> result = new List<T>();

            int batchStart = 0;
            while (batchStart < elements.Count)
            {
                int maxBatchSize = CalculateMaxBatchSize(elements.Cast<IParameters>().ToList(), batchStart, elements.Count);

                // Get the current batch of products that fit within the parameter limit
                var batchOfElements = elements.Skip(batchStart).Take(maxBatchSize).ToList();

                // Add the products in the current batch to the result
                var addedElements = await AddTInBatch(batchOfElements, connection, transaction, addBatchMethod, context);
                result.AddRange(addedElements);

                // Move to the next batch
                batchStart += maxBatchSize;
            }
            return result;
        }
    }
}