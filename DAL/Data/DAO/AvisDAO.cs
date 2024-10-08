using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
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

        private const string _PageQuery = "INSERT INTO Page(PdfUrl, PageNumber, AvisId) VALUES(@PdfUrl, @PageNumber, @AvisId) SELECT SCOPE_IDENTITY();";

        public AvisDAO(IProductDAO productDAO)
        {
            this._productDAO = productDAO;
        }

        public Task Delete(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        //public async Task<Avis?> Get(int id, int permissionLevel)
        //{
        //    using (SqlConnection connection = new SqlConnection(ConnectionString))
        //    {
        //        await connection.OpenAsync();
        //
        //        using (SqlCommand command = new SqlCommand("SELECT * FROM Avis WHERE Id = @Id", connection))
        //        {
        //            command.Parameters.AddWithValue("@Id", id);
        //
        //            using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //            {
        //                if (await reader.ReadAsync())
        //                {
        //                    Avis avis = new Avis(
        //                        reader.GetDateTime(reader.GetOrdinal("ValidFrom")),
        //                        reader.GetDateTime(reader.GetOrdinal("ValidTo")),
        //                        reader.GetString(reader.GetOrdinal("ExternalId"))
        //                    );
        //
        //                    avis.SetId(id);
        //
        //                    return avis;
        //                }
        //                else
        //                {
        //                    throw new NotFoundException("Avis not found");
        //                }
        //            }
        //        }
        //

        private async Task<int> GetIdOfAvisFromExternalId(string externalId)
        {
            if (CachedExtIdToAvisIdRequests.ContainsKey(externalId))
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
    
    public Task<List<Avis>> GetAll(int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task Update(Avis avis, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Add(Avis avis, int companyId, int permissionLevel)
        {
            if (await DoesAvisWithExternalIdExist(avis.ExternalId))
            {
                throw new DALException("Avis with external ID already exists");
            }
            else if(permissionLevel < 2) 
            {
                throw new InsufficientTokenPermission("Permission level too low");
            }
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

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

                            avis.SetId(generatedId);

                            int baseAvisId = await GetIdOfAvisFromExternalId(BaseAvisExternalId);

                            await _productDAO.AddProductsInBatch(avis.Products, connection,transaction, baseAvisId, avis.Id);

                            transaction.Commit();

                            return generatedId;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
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
        public Task Get(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }
    }
}