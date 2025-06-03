using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Diagnostics;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class AvisDAO : DAObject, IAvisDAO
    {
        private Dictionary<string, int> CachedExtIdToAvisIdRequests = new();

        private readonly string BaseAvisExternalId = "base";

        private IProductDAO _productDAO;

        private const string avisQuery = @"
            INSERT INTO Avis (ExternalId, CompanyId, ValidFrom, ValidTo) 
            VALUES (@ExternalId, @CompanyId, @ValidFrom, @ValidTo); 
            SELECT SCOPE_IDENTITY();";

        private const string _GetAvisQuery = @"SELECT 
                A.Id AS AvisId,
                A.ValidFrom,
                A.ValidTo,
                C.Name AS CompanyName,
                P.Id AS ProductId,
                P.Name AS ProductName,
                P.Description,
                P.ImageUrl,
                P.amount,
                PR.Id AS PriceId,
                PR.Price,
	            PR.ProductId AS ProductId,
                PR.CompareUnitString
            FROM[dbo].[Avis]
                    AS A
            JOIN[dbo].[Company]
                    AS C
                ON A.CompanyId = C.Id
            JOIN [dbo].[Price] AS PR
                ON A.Id = PR.AvisId
            JOIN[dbo].[Product] AS P
                ON PR.ProductId = P.Id
            WHERE A.Id = @Id;";

        private const string _PageQuery = "INSERT INTO Page(PdfUrl, PageNumber, AvisId) VALUES(@PdfUrl, @PageNumber, @AvisId) SELECT SCOPE_IDENTITY();";

        public AvisDAO(IProductDAO productDAO, string connectionString) : base(connectionString)
        {
            this._productDAO = productDAO;
        }

        public async Task<bool> Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("delete from avis where Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<int> GetLatestAvisId(int companyId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SELECT TOP 1 a.Id FROM Avis AS a WHERE a.CompanyId = @CompanyId ORDER BY A.[ValidFrom] DESC", connection))
                {
                    command.Parameters.AddWithValue("@CompanyId", companyId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();
                        if (reader.HasRows)
                        {
                            return reader.GetInt32(reader.GetOrdinal("Id"));
                        }
                        else { return -1; }
                    }
                }
            }
        }

        private async Task<int> GetIdOfAvisFromExternalId(string externalId, int? companyId = null)
        {
            if (CachedExtIdToAvisIdRequests.ContainsKey(externalId))
            {
                return CachedExtIdToAvisIdRequests[externalId];
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = "Select Id from avis with (NOLOCK) where ExternalId = @ExternalId";

                if (companyId.HasValue)
                {
                    query += " AND CompanyId = @CompanyId";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
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

        public Task<List<Avis>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task Update(Avis avis)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Add(Avis avis, int companyId)
        {
            if (await DoesAvisWithExternalIdExist(avis.ExternalId))
            {
                throw new DALException("Avis with external ID already exists");
            }

            Console.WriteLine("Avis not already existing");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                Console.WriteLine("Connection opened");

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(avisQuery, connection, transaction))
                        {
                            // Assigning parameters to the SQL command
                            command.Parameters.AddWithValue("@ExternalId", avis.ExternalId);
                            command.Parameters.AddWithValue("@CompanyId", companyId);
                            command.Parameters.AddWithValue("@ValidFrom", avis.ValidFrom);
                            command.Parameters.AddWithValue("@ValidTo", avis.ValidTo);

                            // Execute the query and get the generated ID
                            int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());

                            Console.WriteLine("Temporary avis created with id: " + generatedId);

                            avis.SetId(generatedId);

                            int baseAvisId = await GetIdOfAvisFromExternalId(BaseAvisExternalId, companyId);

                            Console.WriteLine("Found base avis with id: " + baseAvisId);


                            int count = 0;
                            if (avis.Products.Count > 0)
                            {
                                await _productDAO.AddProducts(avis.Products, transaction, connection, baseAvisId, avis.Id, BaseAvisExternalId);
                                count++;
                            }
                            Console.WriteLine("Added " + count + " products");


                            transaction.Commit();

                            Console.WriteLine("Transaction committed");

                            return generatedId;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        Console.WriteLine("Rolled back transaction");
                        throw;
                    }
                }
            }
        }
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

        private async Task<bool> DoesAvisWithExternalIdExist(string externalId)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new("SELECT Id FROM Avis WHERE ExternalId = @ExternalId", connection))
                {
                    command.Parameters.AddWithValue("@ExternalId", externalId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        return await reader.ReadAsync();
                    }
                }
            }
        }

        private async Task SavePages(Avis avis, SqlTransaction transaction, SqlConnection connection)
        {
            foreach (Page page in avis.Pages)
            {
                using (SqlCommand command = new SqlCommand(_PageQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@PdfUrl", page.ImageUrl);
                    command.Parameters.AddWithValue("@PageNumber", page.PageNumber);
                    command.Parameters.AddWithValue("@AvisId", avis.Id);

                    page.SetId(Convert.ToInt32(await command.ExecuteScalarAsync()));
                }
            }
        }
        public async Task<Avis?> Get(int id)
        {
            Avis? avis = null;
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new(_GetAvisQuery, connection))
                {
                    var result = await connection.QueryAsync<Avis, Product, Price, object>(
                    _GetAvisQuery,
                        (avisFromDb, product, price) =>
                        {
                            if (avis == null)
                            {
                                // Initialize the Avis object when it's first encountered in the result
                                avis = new Avis
                                {
                                    Id = avisFromDb.Id,
                                    ValidFrom = avisFromDb.ValidFrom,
                                    ValidTo = avisFromDb.ValidTo,
                                    Products = new List<Product>()
                                };
                            }

                            // Ensure the product is added only once and map the Price for the Product
                            if (product != null)
                            {
                                var existingProduct = avis.Products.FirstOrDefault(p => p.Id == product.Id);
                                if (existingProduct == null)
                                {
                                    product.Prices = new List<Price>();  // Initialize the Prices list for this Product if it's the first time we encounter it
                                    avis.Products.Add(product);
                                }
                                existingProduct = avis.Products.First(p => p.Id == product.Id);

                                if (price != null && !existingProduct.Prices.Any(p => p.Id == price.Id))
                                {
                                    existingProduct.Prices.Add(price);
                                }
                            }

                            return null;
                        },
                        param: new { Id = id },
                        splitOn: "AvisId, ProductId, PriceId"
                    );

                }

            }
            return avis;
        }
    }
}