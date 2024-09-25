using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Dao;
using TIlbudsAvisScraperAPI.Data.Interfaces;
using System.Data.SqlClient;
using DAL.Data.DAO;

namespace TIlbudsAvisScraperAPI.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private string productQuery = "INSERT INTO Product (Id, ExternalId, Name, Description, ImageUrl) " +
                       "VALUES (@Id, @ExternalId, @Name, @Description, @ImageUrl); " +
                       "SELECT SCOPE_IDENTITY();";

        private string priceQuery = "INSERT INTO Price (Id, ProductId, Price) " +
                   "VALUES (@Id, @ProductId, @Price);";

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Product> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task Update(Product product)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Add(Product product)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand(productQuery, connection, transaction);

                        command.Parameters.AddWithValue("@Id", product.Id);
                        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);

                        int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());

                        // Add the prices related to that product
                        foreach (Price price in product.GetPrices())
                        {
                            SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction);

                            priceCommand.Parameters.AddWithValue("@Id", price.Id);
                            priceCommand.Parameters.AddWithValue("@ProductId", generatedId);
                            priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);

                            await priceCommand.ExecuteNonQueryAsync();
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