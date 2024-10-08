using TilbudsAvisLibrary.Entities;
using System.Data.SqlClient;
using DAL.Data.Interfaces;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using DAL.Data.Exceptions;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using DAL.Data.DataTransferObject;

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

        private readonly string _addPriceQuery = "INSERT INTO Price (ProductId, Price, AvisId, CompareUnitString) " +
                        "VALUES (@ProductId, @Price, @AvisId, @CompareUnitString)";
        private readonly string _getPriceAndExternalAvisIdquery = @"SELECT p.*, a.ExternalId FROM Price p JOIN Avis a ON p.AvisId = a.Id WHERE p.ProductId = @ProductId;";


        private readonly string _addNutritionInfoQuery = "INSERT INTO NutritionInfo (ProductId, EnergyKJ, FatPer100G, SaturatedFatPer100G, CarbohydratesPer100G, SugarsPer100G, FiberPer100G, ProteinPer100G, SaltPer100G) " +
                    "VALUES (@ProductId, @EnergyKJ, @FatPer100G, @SaturatedFatPer100G, @CarbohydratesPer100G, @SugarsPer100G, @FiberPer100G, @ProteinPer100G, @SaltPer100G); " +
                    "SELECT SCOPE_IDENTITY();";

        private readonly string _priceAlreadyExistsQuery = @"
                    SELECT a.ExternalId 
                    FROM Price p WITH (NOLOCK)
                    JOIN Avis a WITH (NOLOCK) ON p.AvisId = a.Id 
                    WHERE p.ProductId = @ProductId AND a.ExternalId = @ExternalId;";




        public ProductDAO()
        {
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

        private async Task<NutritionInfo> GetNutritionForProduct(int productId)
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

        private async Task AddNutritionInfo(NutritionInfo nutritionInfo, int productId, SqlConnection connection, SqlTransaction transaction)
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

        private async Task<List<Price>> GetPricesForProduct(int productId)
        {
            List<Price> prices = new List<Price>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(_getPriceAndExternalAvisIdquery, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            float price = (float)reader.GetDouble(reader.GetOrdinal("Price"));
                            string externalAvisId = reader.GetString(reader.GetOrdinal("ExternalId"));

                            string CompareUnitString = reader.GetString(reader.GetOrdinal("CompareUnitString"));
                            Price newPrice = new Price(id, price, externalAvisId, CompareUnitString);
                            prices.Add(newPrice);
                        }
                    }
                }
            }
            return prices;
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

        public Task<int> Add(Product product, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        private async Task<List<Product>> AddProductsInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            return await AddTInBatch(products, connection, transaction, AddProductsBatchInternal);
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

                // Finalize the command text with the constructed rows
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

                            // Collect the product's nutrition info to be inserted later
                            if (product.NutritionInfo != null)
                            {
                                productsWithNutrition.Add((insertedId, product.NutritionInfo));
                            }
                        }
                    }
                }
            }
            await AddNutritionInfosInBatch(products, connection, transaction);

            return addedProducts;
        }
        private async Task<List<Product>> AddNutritionInfosInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction)
        {
            return await AddTInBatch(products, connection, transaction, AddNutritionInfoBatchInternal);
        }

        private async Task<List<Product>> AddNutritionInfoBatchInternal(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            var commandText = new StringBuilder();
            commandText.AppendLine("INSERT INTO NutritionInfo (ProductId, EnergyKJ, FatPer100G, SaturatedFatPer100G, CarbohydratesPer100G, SugarsPer100G, FiberPer100G, ProteinPer100G, SaltPer100G) VALUES ");

            var parameters = new List<string>();
            int parameterIndex = 0;

            foreach (var product in products)
            {
                var nutritionInfo = product.NutritionInfo;
                if (nutritionInfo != null)
                {
                    parameters.Add($"(@ProductId{parameterIndex}, @EnergyKJ{parameterIndex}, @FatPer100G{parameterIndex}, @SaturatedFatPer100G{parameterIndex}, @CarbohydratesPer100G{parameterIndex}, @SugarsPer100G{parameterIndex}, @FiberPer100G{parameterIndex}, @ProteinPer100G{parameterIndex}, @SaltPer100G{parameterIndex})");
                    parameterIndex++;
                }
            }

            if (parameters.Count > 0)
            {
                commandText.AppendLine(string.Join(", ", parameters));
                commandText.AppendLine(";");

                // Create the SqlCommand object here
                using (SqlCommand command = new SqlCommand(commandText.ToString(), connection, transaction))
                {
                    parameterIndex = 0; // Reset the parameter index for adding parameters
                    foreach (var product in products)
                    {
                        var nutritionInfo = product.NutritionInfo;
                        if (nutritionInfo != null)
                        {
                            // Add parameters to command
                            command.Parameters.AddWithValue($"@ProductId{parameterIndex}", product.Id);
                            command.Parameters.AddWithValue($"@EnergyKJ{parameterIndex}", nutritionInfo.EnergyKJ);
                            command.Parameters.AddWithValue($"@FatPer100G{parameterIndex}", nutritionInfo.FatPer100G);
                            command.Parameters.AddWithValue($"@SaturatedFatPer100G{parameterIndex}", nutritionInfo.SaturatedFatPer100G);
                            command.Parameters.AddWithValue($"@CarbohydratesPer100G{parameterIndex}", nutritionInfo.CarbohydratesPer100G);
                            command.Parameters.AddWithValue($"@SugarsPer100G{parameterIndex}", nutritionInfo.SugarsPer100G);
                            command.Parameters.AddWithValue($"@FiberPer100G{parameterIndex}", nutritionInfo.FiberPer100G);
                            command.Parameters.AddWithValue($"@ProteinPer100G{parameterIndex}", nutritionInfo.ProteinPer100G);
                            command.Parameters.AddWithValue($"@SaltPer100G{parameterIndex}", nutritionInfo.SaltPer100G);
                            parameterIndex++;
                        }
                    }
                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }
            }
            return products;
        }

        public async Task<List<Price>> AddPricesInBatch(List<Price> prices, SqlConnection connection, SqlTransaction transaction, int baseAvisId, int avisId, string avisExternalId)
        {
            return await AddTInBatch(prices, connection, transaction, AddPricesInBatchInternal, new BatchContext(baseAvisId, avisId, avisExternalId));
        }

        private async Task<List<Price>> AddPricesInBatchInternal(List<Price> prices, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            throw new NotImplementedException();
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
            // Use batch insert if there are more than 30 products
            if (products.Count() > 30)
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

                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        product.SetId(generatedId);

                        if (product.NutritionInfo != null)
                        {
                            await AddNutritionInfo(product.NutritionInfo, generatedId, connection, transaction);
                        }
                        await CreatePrices(connection, transaction, product, baseAvisId, avisId, avisExternalId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    await CreatePrices(connection, transaction, product, baseAvisId, avisId, avisExternalId);

                    product.SetId((int)checkProduct.Id);
                }
                addedProducts.Add(product);
            }
            return addedProducts;
        }


        private async Task CreatePrices(SqlConnection connection, SqlTransaction transaction, Product product, int baseAvisId, int avisId, string avisExternalId)
        {
            foreach (Price price in product.GetPrices())
            {
                string externalIdToUse = avisExternalId;
                if (price.ExternalAvisId != null)
                {
                    externalIdToUse = price.ExternalAvisId;
                }

                using (SqlCommand priceCommand = new SqlCommand(_addPriceQuery, connection, transaction))
                {
                    priceCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);
                    if (externalIdToUse == "base")
                    {
                        avisId = baseAvisId;
                    }

                    priceCommand.Parameters.AddWithValue("@AvisId", avisId);

                    priceCommand.Parameters.AddWithValue("@CompareUnitString", price.CompareUnitString);

                    price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                }
            }
        }


        private async Task<Product> CreateProductObjectFromReader(SqlDataReader reader)
        {
            int productId = reader.GetInt32(reader.GetOrdinal("Id"));
            string name = reader.GetString(reader.GetOrdinal("Name"));
            string description = reader.GetString(reader.GetOrdinal("Description"));
            string imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
            int externalId = reader.GetInt32(reader.GetOrdinal("ExternalId"));
            float? amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? (float?)null : (float)reader.GetDouble(reader.GetOrdinal("Amount"));

            List<Price> prices = await GetPricesForProduct(productId);
            NutritionInfo nutritionInfo = await GetNutritionForProduct(productId);

            return new Product(prices, productId, name, imageUrl, description, externalId, nutritionInfo, amount);
        }
    }
}