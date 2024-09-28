using DAL.Data.Interfaces;
using System.Data.SqlClient;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class AvisDAO : DAObject, IAvisDAO
    {
        private string avisQuery = @"
            INSERT INTO Avis (AvisExternalId, CompanyId, ValidFrom, ValidTo) 
            VALUES (@AvisExternalId, @CompanyId, @ValidFrom, @ValidTo); 
            SELECT SCOPE_IDENTITY();";

        public Task<int> Add(Avis t)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Avis> Get(int id)
        {
            throw new NotImplementedException();
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
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand(avisQuery, connection, transaction);

                        // Assigning parameters to the SQL command
                        command.Parameters.AddWithValue("@AvisExternalId", avis.ExternalId);
                        command.Parameters.AddWithValue("@CompanyId", companyId);
                        command.Parameters.AddWithValue("@ValidFrom", avis.ValidFrom);
                        command.Parameters.AddWithValue("@ValidTo", avis.ValidTo);

                        // Execute the query and get the generated ID
                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());

                        transaction.Commit();

                        return generatedId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        
    }
}