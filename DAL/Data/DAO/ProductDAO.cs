﻿using TilbudsAvisLibrary.Entities;
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
using TilbudsAvisLibrary.DTO;

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
                                INNER JOIN Price pr ON p.Id = pr.ProductId
                                INNER JOIN Avis a ON pr.AvisId = a.Id
                                INNER JOIN Company c ON a.CompanyId = c.Id
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


        private readonly string _getProduct = @"SELECT 
                   p.Id, p.ExternalId, p.Name, p.Description, p.ImageUrl, p.amount,      
                    c.*,     
					pr.Id, pr.Price as PriceValue, pr.AvisId as ExternalAvisId, pr.CompareUnitString,       
                    a.*         
                FROM 
                    Product p
                INNER JOIN 
                    Price pr ON p.Id = pr.ProductId
                INNER JOIN 
                    Avis a ON a.Id = pr.AvisId
				INNER JOIN 
                    Company c ON a.CompanyId = c.Id
                WHERE 
                    p.id = @Id";

        private readonly string _getProducts = @"
            WITH RankedAvis AS (
    SELECT
        pr.ProductId,
        pr.AvisId,
        pr.Price,
        a.ValidFrom,
        a.ValidTo,
        c.Name AS PriceRetailerName,
        a.ExternalId AS PriceExternalIdAvis,
        ROW_NUMBER() OVER (PARTITION BY pr.ProductId ORDER BY pr.Price DESC) AS RowNum
    FROM
        Price pr
    INNER JOIN
        Avis a ON pr.AvisId = a.Id
    INNER JOIN
        Company c ON a.CompanyId = c.Id
    WHERE
        a.ExternalId != 'base'
        AND a.ValidFrom <= CAST(GETDATE() AS DATE)
        AND a.ValidTo >= CAST(GETDATE() AS DATE)
)

SELECT 
    p.Id AS ProductId, 
    p.ExternalId AS ProductExternalId, 
    p.amount AS ProductAmount, 
    p.Name AS ProductName, 
    p.Description AS ProductDescription, 
    p.ImageUrl AS ProductImageUrl, 
    pr.Price AS Price, 
    a.ValidFrom AS PriceValidFrom, 
    a.ValidTo AS PriceValidTo, 
    c.Name as PriceRetailerName, 
    a.ExternalId as PriceExternalIdAvis,
    nu.CarbohydratesPer100G, 
    nu.EnergyKJ, 
    nu.FatPer100G, 
    nu.FiberPer100G, 
    nu.ProteinPer100G, 
    nu.productId, 
    nu.SaturatedFatPer100G, 
    nu.SugarsPer100G,
    nu.SaltPer100G,
    priceAgg.MaxPrice, 
    priceAgg.MinPrice
FROM 
    Product p
LEFT JOIN 
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
    RankedAvis pr ON pr.ProductId = p.Id
    AND pr.RowNum = 1  -- Only pick the top-ranked Avis based on price
LEFT JOIN 
    Avis a ON pr.AvisId = a.Id
LEFT JOIN 
    Company c on a.CompanyId = c.Id
WHERE 
    a.ExternalId != 'base' 
    AND a.ValidFrom <= CAST(GETDATE() AS DATE) 
    AND a.ValidTo >= CAST(GETDATE() AS DATE)";


        private INutritionInfoDAO _nutritionInfoDAO;
        private IPriceDAO _priceDAO;

        public ProductDAO(INutritionInfoDAO nutritionInfoDAO, IPriceDAO priceDAO, string connectionString) : base(connectionString)
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

            List<Price> prices = await _priceDAO.GetPricesForProduct(productId);
            NutritionInfo nutritionInfo = await _nutritionInfoDAO.GetNutritionForProduct(productId);

            return new Product(prices, productId, name, imageUrl, description, externalId, nutritionInfo, amount);
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

        public async Task<List<ProductDTO>> GetProducts(ProductQueryParameters parameters)
        {
            List<ProductDTO> products = new List<ProductDTO>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                StringBuilder queryBuilder = new StringBuilder(_getProducts);

                if (!string.IsNullOrEmpty(parameters.Retailer))
                {
                    queryBuilder.Append(" AND c.Name IN (SELECT value FROM STRING_SPLIT(@Retailers, ','))");
                }

                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    queryBuilder.Append(" AND (p.Name LIKE '%' + @SearchTerm + '%' OR p.Description LIKE '%' + @SearchTerm + '%')");
                }

                string sortColumn = parameters.SortBy?.ToLower() switch
                {
                    "pricedesc" => "priceAgg.MaxPrice",
                    "priceasc" => "priceAgg.MinPrice",
                    "name" => "p.Name",
                    _ => "p.Id" // Default sorting by Product ID
                };

                string sortDirection = string.IsNullOrEmpty(parameters.SortBy) || !parameters.SortBy.EndsWith("Desc", StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC";

                queryBuilder.Append($" ORDER BY {sortColumn} {sortDirection}");

                queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                {
                    if (!string.IsNullOrEmpty(parameters.Retailer))
                    {
                        command.Parameters.AddWithValue("@Retailers", parameters.Retailer);
                    }

                    if (!string.IsNullOrEmpty(parameters.SearchTerm))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", parameters.SearchTerm);
                    }

                    command.Parameters.AddWithValue("@Offset", parameters.PageNumber * parameters.PageSize);
                    command.Parameters.AddWithValue("@PageSize", parameters.PageSize);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(CreateProductDTO(reader));
                        }
                    }
                }
            }
            return products;
        }


        public async Task<List<Company>> GetProductWithInformationAsync(int productID)
        {
            List<Company> companies = new();

            using SqlConnection connection = new(ConnectionString);
            await connection.OpenAsync();

            var parameters = new { Id = productID };

            _ = await connection.QueryAsync<Product, Company, Price, Avis, object>(
                _getProduct,
                (product, company, price, avis) =>
                {
                    if (!companies.Any(c => c.Id == company.Id))
                    {
                        company.Aviser = new List<Avis>();
                        companies.Add(company);
                    }

                    var currentCompany = companies.First(c => c.Id == company.Id);
                    if (!currentCompany.Aviser.Any(a => a.Id == avis.Id))
                    {
                        currentCompany.Aviser.Add(avis);
                        avis.Products = new List<Product>();
                    }

                    if (!avis.Products.Any(p => p.Id == product.Id))
                    {
                        avis.Products.Add(product);
                    }

                    product.Prices = [price];

                    return null;
                },
                param: parameters,
                splitOn: "Id, Id, Id, Id"
            );

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

            NutritionInfo? nutritionInfo = null;
            if (!reader.IsDBNull(reader.GetOrdinal("EnergyKJ")))
            {
                nutritionInfo = new NutritionInfo
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
            }

            return new Product
            {
                Id = productId,
                Prices = new List<Price> { priceObject },
                Name = name,
                ImageUrl = imageUrl,
                Description = description,
                ExternalId = externalId,
                Amount = (float?)amount,
                NutritionInfo = nutritionInfo
            };
        }

        private ProductDTO CreateProductDTO(SqlDataReader reader)
        {
            int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));
            string name = reader.GetString(reader.GetOrdinal("ProductName"));
            string description = reader.GetString(reader.GetOrdinal("ProductDescription"));
            string imageUrl = reader.GetString(reader.GetOrdinal("ProductImageUrl"));
            double amount = reader.GetDouble(reader.GetOrdinal("ProductAmount"));
            string externalId = reader.GetString(reader.GetOrdinal("ProductExternalId"));

            PriceDTO price = CreatePriceDTO(reader);
            NutritionInfoDTO? nutritionInfo = !reader.IsDBNull(reader.GetOrdinal("EnergyKJ")) ? CreateNutritionInfoDTO(reader) : null;

            return new ProductDTO
            {
                Id = productId,
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                Amount = (float)amount,
                ExternalId = externalId,
                NutritionInfo = nutritionInfo,
                Prices = new List<PriceDTO> { price }
            };
        }

        private PriceDTO CreatePriceDTO(SqlDataReader reader)
        {
            float price = (float)reader.GetDouble(reader.GetOrdinal("Price"));
            DateTime validFrom = reader.GetDateTime(reader.GetOrdinal("PriceValidFrom"));
            DateTime validTo = reader.GetDateTime(reader.GetOrdinal("PriceValidTo"));
            string retailerName = reader.GetString(reader.GetOrdinal("PriceRetailerName"));
            string externalAvisId = reader.GetString(reader.GetOrdinal("PriceExternalIdAvis"));

            return new PriceDTO
            {
                Price = price,
                ValidFrom = validFrom,
                ValidTo = validTo,
                CompanyName = retailerName,
                ExternalAvisId = externalAvisId
            };
        }


        private NutritionInfoDTO CreateNutritionInfoDTO(SqlDataReader reader) => new NutritionInfoDTO
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
        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            int totalCount = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                // Start with the base query for the count
                StringBuilder countQueryBuilder = new StringBuilder(@"
                        SELECT COUNT(DISTINCT p.Id) 
                        FROM Product p
                        LEFT JOIN NutritionInfo nu ON p.Id = nu.productId
                        INNER JOIN (
                            SELECT 
                                pr.ProductId, 
                                MAX(pr.Price) AS MaxPrice, 
                                MIN(pr.Price) AS MinPrice
                            FROM Price pr
                            INNER JOIN Avis a ON pr.AvisId = a.Id
                            WHERE a.ExternalId != 'base' 
                              AND a.ValidFrom <= CAST(GETDATE() AS DATE)
                              AND a.ValidTo >= CAST(GETDATE() AS DATE)
                            GROUP BY pr.ProductId
                        ) priceAgg ON p.Id = priceAgg.ProductId
                        INNER JOIN Price pr ON pr.ProductId = p.Id AND pr.Price = priceAgg.MaxPrice
                        INNER JOIN Avis a ON pr.AvisId = a.Id
                        INNER JOIN Company c ON a.CompanyId = c.Id
                        WHERE a.ExternalId != 'base'
                          AND a.ValidFrom <= CAST(GETDATE() AS DATE) 
                          AND a.ValidTo >= CAST(GETDATE() AS DATE)");

                // Apply filters dynamically
                if (!string.IsNullOrEmpty(parameters.Retailer))
                {
                    countQueryBuilder.Append(" AND c.Name IN (@Retailers)");
                }

                using (SqlCommand command = new SqlCommand(countQueryBuilder.ToString(), connection))
                {
                    // Add retailer parameter if provided
                    if (!string.IsNullOrEmpty(parameters.Retailer))
                    {
                        command.Parameters.AddWithValue("@Retailers", string.Join(",", parameters.Retailer));
                    }

                    // Execute the query and retrieve the total count
                    totalCount = (int)await command.ExecuteScalarAsync();
                }
            }

            return totalCount;
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
