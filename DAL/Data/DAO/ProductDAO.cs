using TilbudsAvisLibrary.Entities;
using System.Data.SqlClient;
using DAL.Data.Interfaces;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using DAL.Data.Exceptions;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using DAL.Data.Batch;
using System.Transactions;
using System.Data.Common;

namespace DAL.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private readonly string _addProductQuery = "INSERT INTO Product (ExternalId, Name, Description, ImageUrl, Amount) " +
                       "VALUES (@ExternalId, @Name, @Description, @ImageUrl, @Amount); " +
                       "SELECT SCOPE_IDENTITY();";

        private readonly string _addProductBatchQuery = @"
                        INSERT INTO Product (ExternalId, Name, Description, ImageUrl, Amount)
                        OUTPUT INSERTED.Id, INSERTED.ExternalId
                         VALUES {0};";

        private INutritionInfoDAO _nutritionInfoDAO;
        private IPriceDAO _priceDAO;

        public ProductDAO(INutritionInfoDAO nutritionInfoDAO, IPriceDAO priceDAO)
        {
            this._nutritionInfoDAO = nutritionInfoDAO;
            this._priceDAO = priceDAO;
        }

        public async Task<bool> DeleteNegativeExternalIds()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("delete from product where ExternalId < 0", connection))
                {
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteOnExternalId(int externalId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(DeleteFromTableQueryWhereCondition("product", "ExternalId"), connection))
                {
                    command.Parameters.AddWithValue("@ExternalId", externalId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        public Task Delete(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public async Task<Product?> GetProductFromExernalId(int inputExternalId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("SELECT * FROM Product WITH (NOLOCK) WHERE ExternalId = @ExternalId", connection))
                    {
                        command.Parameters.AddWithValue("@ExternalId", inputExternalId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                return await CreateProductObjectFromReader(reader);
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                throw ex;
            }
        }



        public async Task<Product?> Get(int id, int permissionLevel)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    SqlCommand command = new SqlCommand(GetAllFromTableQueryOnCondition("Product", "Id"), connection);
                    command.Parameters.AddWithValue("@Id", id);

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();

                        return await CreateProductObjectFromReader(reader);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    throw ex;
                }
            }
        }



        public async Task<List<Product>> GetAll(int permissionLevel)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();


                using (SqlCommand command = new SqlCommand("select * from product where 1 = 1", connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(await CreateProductObjectFromReader(reader));
                        }
                    }
                }
            }
            return products;
        }

        public async Task<bool> Update(Product product, int permissionLevel)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("update product set ExternalId = @ExternalId, Name = @Name, Description = @Description, ImageUrl = @ImageUrl, Amount = @Amount where Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);
                    command.Parameters.AddWithValue("@Amount", product.Amount);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<int> Add(Product product, int permissionLevel, int baseAvisId, int avisId, string avisBaseExternalId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand(_addProductQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);
                        command.Parameters.AddWithValue("@Amount", product.Amount);

                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        product.SetId(generatedId);

                        if (product.NutritionInfo != null)
                        {
                            await _nutritionInfoDAO.AddNutritionInfo(product.NutritionInfo, generatedId, connection, transaction);
                        }

                        await _priceDAO.AddPricesForProduct(connection, transaction, product, baseAvisId, avisId, avisBaseExternalId);

                        return (int)product.Id;
                    }
                }
            }
        }

        private async Task<List<Product>> AddProductsInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            return await AddElementsWithMaxBatchSize(products, connection, transaction, context, AddProductsBatchInternal);
        }
        private async Task<List<Product>> AddProductsBatchInternal(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            List<Product> addedProducts = new();
            List<(int productId, NutritionInfo nutritionInfo)> productsWithNutrition = new();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.Transaction = transaction;

                command.CommandText = string.Format(_addProductBatchQuery, "{0}");

                List<string> rows = new();

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];

                    // Construct the parameterized value string
                    rows.Add($"(@ExternalId{i}, @Name{i}, @Description{i}, @ImageUrl{i}, @Amount{i})");

                    command.Parameters.AddWithValue($"@ExternalId{i}", product.ExternalId);
                    command.Parameters.AddWithValue($"@Name{i}", product.Name);
                    command.Parameters.AddWithValue($"@Description{i}", product.Description);
                    command.Parameters.AddWithValue($"@ImageUrl{i}", product.ImageUrl);
                    command.Parameters.AddWithValue($"@Amount{i}", product.Amount);
                }
                command.CommandText = string.Format(_addProductBatchQuery, string.Join(", ", rows));

                // Execute the command and read the inserted IDs
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Create a dictionary to map external IDs to products
                    Dictionary<int, Product> productMap = products.ToDictionary(p => p.ExternalId);

                    // Read the inserted IDs and map them back to the products
                    while (await reader.ReadAsync())
                    {
                        int insertedId = reader.GetInt32(0);
                        int externalId = reader.GetInt32(1);

                        // Map the inserted ID back to the original product
                        if (productMap.TryGetValue(externalId, out var product))
                        {
                            product.SetId(insertedId);
                            addedProducts.Add(product);

                            if (product.NutritionInfo != null)
                            {
                                productsWithNutrition.Add((insertedId, product.NutritionInfo));
                            }
                        }
                    }
                }
            }
            await _nutritionInfoDAO.AddNutritionInfosInBatch(products, connection, transaction);
            await _priceDAO.AddPricesForProducts(connection, transaction, products, context.BaseId, context.Id, context.ExternalId);

            return addedProducts;
        }



        public async Task<List<Product>> AddProducts(List<Product> products, int baseAvisId, int avisId, string avisExternalId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    BatchContext context = new(baseAvisId, avisId, avisExternalId);
                    List<Product> addedProducts = await AddProducts(products, transaction, connection, baseAvisId, avisId, avisExternalId);
                    transaction.Commit();
                    return addedProducts;
                }
            }
        }

        public async Task<List<Product>> AddProducts(List<Product> products, SqlTransaction transaction, SqlConnection connection, int baseAvisId, int avisId, string avisExternalId)
        {
            // Use batch insert if there are more than 10 products
            if (products.Count() > 10)
            {
                var batchContext = new BatchContext(baseAvisId, avisId, avisExternalId);
                return await AddProductsInBatch(products, connection, transaction, batchContext);
            }

            List<Product> addedProducts = new();

            foreach (Product product in products)
            {
                Product? checkProduct = await GetProductFromExernalId(product.ExternalId);
                if (checkProduct == null)
                {
                    try
                    {
                        SqlCommand command = new SqlCommand(_addProductQuery, connection, transaction);

                        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);
                        command.Parameters.AddWithValue("@Amount", product.Amount);

                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        product.SetId(generatedId);

                        if (product.NutritionInfo != null)
                        {
                            await _nutritionInfoDAO.AddNutritionInfo(product.NutritionInfo, generatedId, connection, transaction);
                        }
                        await _priceDAO.AddPricesForProduct(connection, transaction, product, baseAvisId, avisId, avisExternalId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    await _priceDAO.AddPricesForProduct(connection, transaction, product, baseAvisId, avisId, avisExternalId);

                    product.SetId((int)checkProduct.Id);
                }
                addedProducts.Add(product);
            }
            return addedProducts;
        }

        private async Task<Product> CreateProductObjectFromReader(SqlDataReader reader)
        {
            int productId = reader.GetInt32(reader.GetOrdinal("Id"));
            string name = reader.GetString(reader.GetOrdinal("Name"));
            string description = reader.GetString(reader.GetOrdinal("Description"));
            string imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
            int externalId = reader.GetInt32(reader.GetOrdinal("ExternalId"));
            float? amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? null : (float)reader.GetDouble(reader.GetOrdinal("Amount"));

            List<Price> prices = await _priceDAO.GetPricesForProduct(productId);
            NutritionInfo nutritionInfo = await _nutritionInfoDAO.GetNutritionForProduct(productId);

            return new Product(prices, productId, name, imageUrl, description, externalId, nutritionInfo, amount);
        }

    }
}