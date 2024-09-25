using DAL.Data.Interfaces;
using System.Data.SqlClient;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class AvisDAO : IAvisDAO
    {
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

        public async Task<int> Add(Avis avis)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand(productQuery, connection, transaction);

                        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);

                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());

                        // Add the prices related to that product
                        foreach (Price price in product.GetPrices())
                        {
                            SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction);

                            priceCommand.Parameters.AddWithValue("@ProductId", generatedId);
                            priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);

                            price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                        }

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