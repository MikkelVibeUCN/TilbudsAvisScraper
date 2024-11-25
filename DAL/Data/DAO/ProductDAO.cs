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
using TilbudsAvisLibrary;
using Emgu.CV;
using System.Data;
using Dapper;

namespace DAL.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private readonly string _addProductQuery = "INSERT INTO Product (ExternalId, Name, Description, ImageUrl, Amount, CompanyId) " +
                       "VALUES (@ExternalId, @Name, @Description, @ImageUrl, @Amount, @CompanyId); " +
                       "SELECT SCOPE_IDENTITY();";

        private readonly string _addProductBatchQuery = @"
                        INSERT INTO Product (ExternalId, Name, Description, ImageUrl, Amount, CompanyId)
                        OUTPUT INSERTED.Id, INSERTED.ExternalId
                         VALUES {0};";

        private readonly string _getRetailersWithValidProducts = @"SELECT DISTINCT c.Name AS RetailerName
                                FROM Product p
                                INNER JOIN Company c ON p.CompanyId = c.Id
                                INNER JOIN Price pr ON p.Id = pr.ProductId
                                INNER JOIN Avis a ON pr.AvisId = a.Id
                                WHERE a.ExternalId != 'base'
                                AND a.ValidFrom <= CAST(GETDATE() AS DATE)
                                AND a.ValidTo >= CAST(GETDATE() AS DATE)";

        private readonly string _getValidProductsCount = @"SELECT COUNT(*) AS ProductCount
            FROM Product p
            INNER JOIN Company c ON p.CompanyId = c.Id
            INNER JOIN Price pr ON p.Id = pr.ProductId
            INNER JOIN NutritionInfo nu ON p.Id = nu.ProductId
            INNER JOIN Avis a ON pr.AvisId = a.Id
            WHERE a.ExternalId != 'base'
            AND a.ValidFrom <= CAST(GETDATE() AS DATE)
            AND a.ValidTo >= CAST(GETDATE() AS DATE)";

        private readonly string _getProducts = @"
            SELECT 
                p.Id, 
                p.ExternalId AS ProductExternalId, 
                p.amount, 
                p.Name, 
                p.Description, 
                p.ImageUrl, 
                pr.Price AS Price, 
                a.ValidFrom AS PriceValidFrom, 
                a.ValidTo AS PriceValidTo, 
                c.Name AS RetailerName, 
                a.ExternalId AS ExternalIdAvis,
                nu.CarbohydratesPer100G, 
                nu.EnergyKJ, 
                nu.FatPer100G, 
                nu.FiberPer100G, 
                nu.ProteinPer100G, 
                nu.productId, 
                nu.SaturatedFatPer100G, 
                nu.SugarsPer100G,
                nu.SaltPer100G,
                -- Price aggregation
                priceAgg.MaxPrice, 
                priceAgg.MinPrice
            FROM 
                Product p
            INNER JOIN 
                Company c ON p.CompanyId = c.Id
            INNER JOIN 
                NutritionInfo nu ON p.Id = nu.productId
            INNER JOIN 
                (SELECT 
                     pr.ProductId, 
                     MAX(pr.Price) AS MaxPrice, 
                     MIN(pr.Price) AS MinPrice
                 FROM 
                     Price pr
                 INNER JOIN 
                     Avis a ON pr.AvisId = a.Id
                 WHERE 
                     a.ExternalId != 'base' 
                     AND a.ValidFrom <= CAST(GETDATE() AS DATE)
                     AND a.ValidTo >= CAST(GETDATE() AS DATE)
                 GROUP BY 
                     pr.ProductId
                ) priceAgg ON p.Id = priceAgg.ProductId
            INNER JOIN 
                Price pr ON pr.ProductId = p.Id AND pr.Price = priceAgg.MaxPrice
            INNER JOIN 
                Avis a ON pr.AvisId = a.Id
            WHERE 
                a.ExternalId != 'base'
                AND a.ValidFrom <= CAST(GETDATE() AS DATE) 
                AND a.ValidTo >= CAST(GETDATE() AS DATE)";


        private INutritionInfoDAO _nutritionInfoDAO;
        private IPriceDAO _priceDAO;

        public ProductDAO(INutritionInfoDAO nutritionInfoDAO, IPriceDAO priceDAO)
        {
            this._nutritionInfoDAO = nutritionInfoDAO;
            this._priceDAO = priceDAO;
        }

        public async Task<bool> DeleteTestProducts()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("delete from product where Name = 'Cool name'", connection))
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

        public async Task<Product?> GetProductFromExernalIdAndCompanyId(string inputExternalId, int companyId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("SELECT * FROM Product WITH (NOLOCK) WHERE ExternalId = @ExternalId AND CompanyId = @CompanyId", connection))
                    {
                        command.Parameters.AddWithValue("@ExternalId", inputExternalId);
                        command.Parameters.AddWithValue("@ExternalId", companyId);

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


                using (SqlCommand command = new SqlCommand("select * from product", connection))
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

        public async Task<int> Add(Product product, int baseAvisId, int avisId, string avisBaseExternalId, int companyId)
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
                        command.Parameters.AddWithValue("@CompanyId", companyId);

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

                    rows.Add($"(@ExternalId{i}, @Name{i}, @Description{i}, @ImageUrl{i}, @Amount{i}, @CompanyId{i})");

                    command.Parameters.AddWithValue($"@ExternalId{i}", product.ExternalId);
                    command.Parameters.AddWithValue($"@Name{i}", product.Name);
                    command.Parameters.AddWithValue($"@Description{i}", product.Description);
                    command.Parameters.AddWithValue($"@ImageUrl{i}", product.ImageUrl);
                    command.Parameters.AddWithValue($"@Amount{i}", product.Amount);
                    command.Parameters.AddWithValue($"@CompanyId{i}", context.CompanyId);
                }
                command.CommandText = string.Format(_addProductBatchQuery, string.Join(", ", rows));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    try
                    {
                        Dictionary<string, Product> productMap = products.ToDictionary(p => p.ExternalId);

                        while (await reader.ReadAsync())
                        {
                            int insertedId = reader.GetInt32(0);
                            string externalId = reader.GetString(1);

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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        throw;
                    }
                    
                }
            }
            await _nutritionInfoDAO.AddNutritionInfosInBatch(products, connection, transaction);
            await _priceDAO.AddPricesForProducts(connection, transaction, products, context.BaseId, context.Id, context.ExternalId);

            return addedProducts;
        }



        public async Task<List<Product>> AddProducts(List<Product> products, int baseAvisId, int avisId, string avisExternalId, int companyId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    BatchContext context = new(baseAvisId, avisId, avisExternalId);
                    List<Product> addedProducts = await AddProducts(products, transaction, connection, baseAvisId, avisId, avisExternalId, companyId);
                    transaction.Commit();
                    return addedProducts;
                }
            }
        }

        public async Task<List<Product>> RemoveExistingProductsAsync(List<Product> products, SqlConnection connection, SqlTransaction transaction, int baseAvisId, int avisId, string avisExternalId, int companyId)
        {
            List<string> externalIds = products.Select(p => p.ExternalId).ToList();

            var existingProducts = await CheckExistingProductIdsAsync(externalIds, companyId, connection, transaction);

            List<Product> removedProducts = new List<Product>(); 
            List<Product> productsToAdd = new List<Product>();

            try
            {
                foreach (var product in products)
                {
                    if (existingProducts.ContainsKey(product.ExternalId))
                    {
                        removedProducts.Add(product); 

                        var priceToRemove = product.Prices.FirstOrDefault(p => p.ExternalAvisId == avisExternalId);
                        if (priceToRemove != null)
                        {
                            product.RemovePrice(priceToRemove);
                        }

                        product.SetId(existingProducts[product.ExternalId]); 
                    }
                    else
                    {
                        productsToAdd.Add(product); 
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            await _priceDAO.AddPricesForProducts(connection, transaction, removedProducts, baseAvisId, avisId, avisExternalId);

            return productsToAdd;
        }


        public async Task<List<Product>> AddProducts(List<Product> products, SqlTransaction transaction, SqlConnection connection, int baseAvisId, int avisId, string avisExternalId, int companyId)
        {
            products = await RemoveExistingProductsAsync(products, connection, transaction, baseAvisId, avisId, avisExternalId, companyId);

            if (products.Count() > 10)
            {
                var batchContext = new BatchContext(baseAvisId, avisId, avisExternalId, companyId);
                return await AddProductsInBatch(products, connection, transaction, batchContext);
            }

            List<Product> addedProducts = new();

            foreach (Product product in products)
            {
                Product? checkProduct = await GetProductFromExernalIdAndCompanyId(product.ExternalId, companyId);
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
            string externalId = reader.GetString(reader.GetOrdinal("ExternalId"));
            float? amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? null : (float)reader.GetDouble(reader.GetOrdinal("Amount"));
            int companyId = reader.GetInt32(reader.GetOrdinal("CompanyId"));

            List<Price> prices = await _priceDAO.GetPricesForProduct(productId);
            NutritionInfo nutritionInfo = await _nutritionInfoDAO.GetNutritionForProduct(productId);

            return new Product(prices, productId, name, imageUrl, description, externalId, nutritionInfo, amount, companyId);
        }

        public async Task<Dictionary<string, int>> CheckExistingProductIdsAsync(List<string> externalIds, int companyId, SqlConnection connection, SqlTransaction transaction)
        {
            // Dictionary to hold existing external IDs and their corresponding database IDs
            Dictionary<string, int> existingProducts = new Dictionary<string, int>();

            string sqlQuery = $"SELECT ExternalId, Id FROM Product WHERE ExternalId IN ({string.Join(", ", externalIds.Select((id, index) => $"@ExternalId{index}"))}) AND CompanyId = @CompanyId";

            using (SqlCommand command = new SqlCommand(sqlQuery, connection, transaction))
            {
                for (int i = 0; i < externalIds.Count; i++)
                {
                    command.Parameters.AddWithValue($"@ExternalId{i}", externalIds[i]);
                }
                command.Parameters.AddWithValue("CompanyId", companyId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string externalId = reader.GetString(0);
                        int id = reader.GetInt32(1); 
                        existingProducts[externalId] = id;
                    }
                }
            }
            return existingProducts; 
        }

        public async Task<List<Company>> GetAllProdudctsWithInformationFromCompany(ProductQueryParameters parameters)
        {
            List<Company> companies = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                StringBuilder queryBuilder = new StringBuilder(_getProducts); 

                if (!string.IsNullOrEmpty(parameters.Retailer))
                {
                    queryBuilder.Append(" AND c.Name IN (@Retailers)");
                }

                string sortColumn = string.IsNullOrEmpty(parameters.SortBy)
                    ? "p.Id"  // Default sorting by Product ID when SortBy is null or empty
                    : parameters.SortBy.ToLower() switch
                    {
                        var s when s.StartsWith("price") => parameters.SortBy.EndsWith("Desc", StringComparison.OrdinalIgnoreCase) ? "priceAgg.MaxPrice" : "priceAgg.MinPrice",  // Use MaxPrice for DESC, MinPrice for ASC
                        var s when s.StartsWith("name") => "p.Name",  // Sorting by product name
                        _ => "p.Id"  // Default to sorting by Product ID
                    };

                string sortDirection = string.IsNullOrEmpty(parameters.SortBy)
                    ? "ASC"  
                    : parameters.SortBy.EndsWith("Desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

                queryBuilder.Append($" ORDER BY {sortColumn} {sortDirection}");

                queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                {
                    if (!string.IsNullOrEmpty(parameters.Retailer))
                    {
                        command.Parameters.AddWithValue("@Retailers", string.Join(",", parameters.Retailer));
                    }

                    command.Parameters.AddWithValue("@Offset", (parameters.PageNumber) * parameters.PageSize);
                    command.Parameters.AddWithValue("@PageSize", parameters.PageSize);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        Dictionary<string, Company> companyMap = new();

                        while (await reader.ReadAsync())
                        {
                            string retailerName = reader.GetString(reader.GetOrdinal("RetailerName"));
                            string avisExternalId = reader.GetString(reader.GetOrdinal("ExternalIdAvis"));

                            if (!companyMap.ContainsKey(retailerName))
                            {
                                var company = new Company(retailerName);
                                companyMap[retailerName] = company;
                            }
                            Company currentCompany = companyMap[retailerName];

                            Avis? avis = currentCompany.Aviser.FirstOrDefault(a => a.ExternalId == avisExternalId);

                            if (avis == null)
                            {
                                avis = CreateAvis(reader);
                                avis.ExternalId = avisExternalId;
                                currentCompany.AddAvis(avis);
                            }

                            avis.AddProduct(CreateProduct(reader));
                        }
                        companies = companyMap.Values.ToList();
                    }
                }
            }
            return companies;
        }



        private Avis CreateAvis(SqlDataReader reader)
        {
            DateTime validFrom = reader.GetDateTime(reader.GetOrdinal("PriceValidFrom"));
            DateTime validTo = reader.GetDateTime(reader.GetOrdinal("PriceValidTo"));


            return new Avis(validFrom, validTo);
        }

        private Product CreateProduct(SqlDataReader reader)
        {
            int productId = reader.GetInt32(reader.GetOrdinal("Id"));
            string name = reader.GetString(reader.GetOrdinal("Name"));
            string description = reader.GetString(reader.GetOrdinal("Description"));
            string imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
            double amount = reader.GetDouble(reader.GetOrdinal("amount"));
            string externalId = reader.GetString(reader.GetOrdinal("ProductExternalId"));

            float price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : (float)reader.GetDouble(reader.GetOrdinal("Price"));

            Price priceObject = new Price(price);

            NutritionInfo nutritionInfo = new NutritionInfo
            {
                EnergyKJ = (float)reader.GetDouble(reader.GetOrdinal("EnergyKJ")),
                FatPer100G = (float)reader.GetDouble(reader.GetOrdinal("FatPer100G")),
                SaturatedFatPer100G = (float)reader.GetDouble(reader.GetOrdinal("SaturatedFatPer100G")),
                CarbohydratesPer100G = (float)reader.GetDouble(reader.GetOrdinal("CarbohydratesPer100G")),
                SugarsPer100G = (float)reader.GetDouble(reader.GetOrdinal("SugarsPer100G")),
                FiberPer100G = (float)reader.GetDouble(reader.GetOrdinal("FiberPer100G")),
                ProteinPer100G = (float)reader.GetDouble(reader.GetOrdinal("ProteinPer100G")),
                SaltPer100G = (float)reader.GetDouble(reader.GetOrdinal("SaltPer100G"))
            };

            return new Product
            {
                Id = productId,
                Prices = new List<Price> { priceObject },  // Correcting this to use List<Price>
                Name = name,
                ImageUrl = imageUrl,
                Description = description,
                ExternalId = externalId,
                Amount = (float?)amount,  // Casting to nullable float if necessary
                NutritionInfo = nutritionInfo
            };
        }


        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            StringBuilder builder = new StringBuilder(_getValidProductsCount);

            if (!string.IsNullOrEmpty(parameters.Retailer))
            {
                builder.Append(" AND c.Name IN (@Retailers)");
            }
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                return await connection.QueryFirstOrDefaultAsync<int>(builder.ToString());
            }
            catch (Exception e)
            {

                throw new Exception("Error retrieving productcount message was: " + e.Message);
            }

        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSearch(ProductQueryParameters parameters)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                return await connection.QueryAsync<string>(_getRetailersWithValidProducts);
            }
            catch (Exception e)
            {

                throw new Exception("Error retrieving retailernames message was: " + e.Message);
            }
        }
    }
}
