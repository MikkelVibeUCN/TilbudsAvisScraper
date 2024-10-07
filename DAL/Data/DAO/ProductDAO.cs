using TilbudsAvisLibrary.Entities;
using System.Data.SqlClient;
using DAL.Data.Interfaces;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using DAL.Data.Exceptions;
using System.Collections;
using System.Runtime.CompilerServices;

namespace DAL.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private readonly string _addProductQuery = "INSERT INTO Product (ExternalId, Name, Description, ImageUrl, Amount) " +
                       "VALUES (@ExternalId, @Name, @Description, @ImageUrl, @Amount); " +
                       "SELECT SCOPE_IDENTITY();";

        private readonly string _addPriceQuery = "INSERT INTO Price (ProductId, Price, AvisId, CompareUnitString) " +
                   "VALUES (@ProductId, @Price, @AvisId, @CompareUnitString)";


        private readonly string _addNutritionInfoQuery = "INSERT INTO NutritionInfo (ProductId, EnergyKJ, FatPer100G, SaturatedFatPer100G, CarbohydratesPer100G, SugarsPer100G, FiberPer100G, ProteinPer100G, SaltPer100G) " +
                    "VALUES (@ProductId, @EnergyKJ, @FatPer100G, @SaturatedFatPer100G, @CarbohydratesPer100G, @SugarsPer100G, @FiberPer100G, @ProteinPer100G, @SaltPer100G); " +
                    "SELECT SCOPE_IDENTITY();";

        private readonly string _priceAlreadyExistsQuery = @"
            SELECT a.ExternalId 
            FROM Price p WITH (NOLOCK)
            JOIN Avis a WITH (NOLOCK) ON p.AvisId = a.Id 
            WHERE p.ProductId = @ProductId AND a.ExternalId = @ExternalId;";

        
        private Dictionary<string, int> CachedExtIdToAvisIdRequests = new();

        private string GetAllFromTableQuery(string tableName, string identifer)
        {
            return $"SELECT * FROM {tableName} where {identifer} = @{identifer};";
        }
        public ProductDAO()
        {
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
                    SqlCommand command = new SqlCommand(GetAllFromTableQuery("Product", "Id"), connection);
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

                
                using (SqlCommand command = new SqlCommand(GetAllFromTableQuery("Price", "ProductId"), connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            float price = (float)reader.GetDouble(reader.GetOrdinal("Price"));

                            string avisExternalId = await GetAvisExternalIdFromId(reader.GetInt32(reader.GetOrdinal("AvisId")));

                            string CompareUnitString = reader.GetString(reader.GetOrdinal("CompareUnitString"));
                            Price newPrice = new Price(id, price, avisExternalId, CompareUnitString);
                            prices.Add(newPrice);
                        }
                    }
                }
            }
            return prices;
        }
        // Move to avisdao
        private async Task<string> GetAvisExternalIdFromId(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("select ExternalId from avis where Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetString(reader.GetOrdinal("ExternalId"));
                        }
                        throw new Exception("Avis not found");
                    }
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

        public Task<int> Add(Product product, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Product>> AddProducts(IEnumerable<Product> products, Avis avis, SqlTransaction transaction, SqlConnection connection)
        {
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

                        if(product.NutritionInfo != null)
                        {
                            await AddNutritionInfo(product.NutritionInfo, generatedId, connection, transaction);
                        }
                        await CreatePrices(avis, connection, transaction, product);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    await CreatePrices(avis, connection, transaction, product);

                    product.SetId((int)checkProduct.Id);
                }
                addedProducts.Add(product);
            }
            return addedProducts;
        }

        private async Task<bool> PriceAlreadyExists(string avisExternalId, Price price, int productId)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(_priceAlreadyExistsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@ExternalId", avisExternalId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        private async Task CreatePrices(Avis avis, SqlConnection connection, SqlTransaction transaction, Product product)
        {
            foreach (Price price in product.GetPrices())
            {
                string externalIdToUse = avis.ExternalId;
                if (price.ExternalAvisId != null)
                {
                    externalIdToUse = price.ExternalAvisId;
                }

                if (!await PriceAlreadyExists(externalIdToUse, price, (int)product.Id))
                {
                    using (SqlCommand priceCommand = new SqlCommand(_addPriceQuery, connection, transaction))
                    {
                        priceCommand.Parameters.AddWithValue("@ProductId", product.Id);
                        priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);
                        int avisId = avis.Id;
                        if (externalIdToUse == "base")
                        {
                            avisId = await GetIdOfAvisFromExternalId(externalIdToUse);
                        }

                        priceCommand.Parameters.AddWithValue("@AvisId", avisId);
                        
                        priceCommand.Parameters.AddWithValue("@CompareUnitString", price.CompareUnitString);

                        price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                    }
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

        private async Task<int> GetIdOfAvisFromExternalId(string externalId)
        {
            if(CachedExtIdToAvisIdRequests.ContainsKey(externalId))
            {
                return CachedExtIdToAvisIdRequests[externalId];
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("Select Id from avis with (NOLOCK) where ExternalId = @ExternalId", connection))
                {
                    command.Parameters.AddWithValue("@ExternalId", externalId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();
                        if (reader.HasRows)
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            CachedExtIdToAvisIdRequests.Add(externalId, id);
                            return id;
                        }
                        else { return -1; }
                    }
                }
            }
        }
    }
}