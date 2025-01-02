using DAL.Data.Batch;
using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class NutritionInfoDAO : DAObject, INutritionInfoDAO
    {
        private readonly string _addNutritionInfoQuery = "INSERT INTO NutritionInfo (ProductId, EnergyKJ, FatPer100G, SaturatedFatPer100G, CarbohydratesPer100G, SugarsPer100G, FiberPer100G, ProteinPer100G, SaltPer100G) " +
                    "VALUES (@ProductId, @EnergyKJ, @FatPer100G, @SaturatedFatPer100G, @CarbohydratesPer100G, @SugarsPer100G, @FiberPer100G, @ProteinPer100G, @SaltPer100G); " +
                    "SELECT SCOPE_IDENTITY();";

        public NutritionInfoDAO(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<Product>> AddNutritionInfosInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction)
        {
            return await AddTInBatch(products, connection, transaction, AddNutritionInfoBatchInternal);
        }

        private async Task<List<Product>> AddNutritionInfoBatchInternal(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            var commandText = new StringBuilder();
            commandText.AppendLine("INSERT INTO NutritionInfo (ProductId, EnergyKJ, FatPer100G, SaturatedFatPer100G, CarbohydratesPer100G, SugarsPer100G, FiberPer100G, ProteinPer100G, SaltPer100G) VALUES ");

            List<string> rows = new();

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var nutritionInfo = product.NutritionInfo;
                if (nutritionInfo != null)
                {
                    rows.Add($"(@ProductId{i}, @EnergyKJ{i}, @FatPer100G{i}, @SaturatedFatPer100G{i}, @CarbohydratesPer100G{i}, @SugarsPer100G{i}, @FiberPer100G{i}, @ProteinPer100G{i}, @SaltPer100G{i})");
                }
            }

            if (rows.Count > 0)
            {
                commandText.AppendLine(string.Join(", ", rows));
                commandText.AppendLine(";");

                using (SqlCommand command = new SqlCommand(commandText.ToString(), connection, transaction))
                {
                    for (int i = 0; i < products.Count; i++)
                    {
                        var product = products[i];
                        var nutritionInfo = product.NutritionInfo;
                        if (nutritionInfo != null)
                        {
                            command.Parameters.AddWithValue($"@ProductId{i}", product.Id);
                            command.Parameters.AddWithValue($"@EnergyKJ{i}", nutritionInfo.EnergyKJ);
                            command.Parameters.AddWithValue($"@FatPer100G{i}", nutritionInfo.FatPer100G);
                            command.Parameters.AddWithValue($"@SaturatedFatPer100G{i}", nutritionInfo.SaturatedFatPer100G);
                            command.Parameters.AddWithValue($"@CarbohydratesPer100G{i}", nutritionInfo.CarbohydratesPer100G);
                            command.Parameters.AddWithValue($"@SugarsPer100G{i}", nutritionInfo.SugarsPer100G);
                            command.Parameters.AddWithValue($"@FiberPer100G{i}", nutritionInfo.FiberPer100G);
                            command.Parameters.AddWithValue($"@ProteinPer100G{i}", nutritionInfo.ProteinPer100G);
                            command.Parameters.AddWithValue($"@SaltPer100G{i}", nutritionInfo.SaltPer100G);
                        }
                    }
                    await command.ExecuteNonQueryAsync();
                }
            }
            return products;
        }

        public async Task AddNutritionInfo(NutritionInfo nutritionInfo, int productId, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(_addNutritionInfoQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@EnergyKJ", nutritionInfo.EnergyKJ);
                    command.Parameters.AddWithValue("@FatPer100G", nutritionInfo.FatPer100G);
                    command.Parameters.AddWithValue("@SaturatedFatPer100G", nutritionInfo.SaturatedFatPer100G);
                    command.Parameters.AddWithValue("@CarbohydratesPer100G", nutritionInfo.CarbohydratesPer100G);
                    command.Parameters.AddWithValue("@SugarsPer100G", nutritionInfo.SugarsPer100G);
                    command.Parameters.AddWithValue("@FiberPer100G", nutritionInfo.FiberPer100G);
                    command.Parameters.AddWithValue("@ProteinPer100G", nutritionInfo.ProteinPer100G);
                    command.Parameters.AddWithValue("@SaltPer100G", nutritionInfo.SaltPer100G);

                    await command.ExecuteScalarAsync();
                }
            }
            catch (Exception e)
            {
                throw new DALException("Error adding nutrition info");
            }
        }
        public async Task<NutritionInfo> GetNutritionForProduct(int productId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM NutritionInfo WHERE ProductId = @ProductId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                float energyKJ = (float)reader.GetDouble(reader.GetOrdinal("EnergyKJ"));
                                float fat = (float)reader.GetDouble(reader.GetOrdinal("FatPer100G"));
                                float satFats = (float)reader.GetDouble(reader.GetOrdinal("SaturatedFatPer100G"));
                                float carbohydrates = (float)reader.GetDouble(reader.GetOrdinal("CarbohydratesPer100G"));
                                float sugars = (float)reader.GetDouble(reader.GetOrdinal("SugarsPer100G"));
                                float fiber = (float)reader.GetDouble(reader.GetOrdinal("FiberPer100G"));
                                float protein = (float)reader.GetDouble(reader.GetOrdinal("ProteinPer100G"));
                                float salt = (float)reader.GetDouble(reader.GetOrdinal("SaltPer100G"));

                                return new NutritionInfo(energyKJ, fat, carbohydrates, sugars, fiber, protein, salt);
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new DALException("Error getting nutrition info");
            }
        }
    }
}
