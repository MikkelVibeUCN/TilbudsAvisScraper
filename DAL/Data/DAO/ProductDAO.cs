using TilbudsAvisLibrary.Entities;
using System.Data.SqlClient;
using DAL.Data.Interfaces;

namespace DAL.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private string productQuery = "INSERT INTO Product (ExternalId, Name, Description, ImageUrl) " +
                       "VALUES (@ExternalId, @Name, @Description, @ImageUrl); " +
                       "SELECT SCOPE_IDENTITY();";

        private string priceQuery = "INSERT INTO Price (ProductId, Price) " +
                   "VALUES (@ProductId, @Price)" +
                    "SELECT SCOPE_IDENTITY();";

        public ProductDAO()
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                Console.WriteLine("Connection opened");

            }
            Console.WriteLine("Connection closed");
        }

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

        public Task<int> Add(Product product)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Product>> AddProducts(IEnumerable<Product> products, int avisId)
        {
            List<Product> addedProducts = new();
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    foreach(Product product in products)
                    {
                        try
                        {
                            SqlCommand command = new SqlCommand(productQuery, connection, transaction);

                            command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                            command.Parameters.AddWithValue("@Name", product.Name);
                            command.Parameters.AddWithValue("@Description", product.Description);
                            command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);

                            product.SetId(Convert.ToInt32(await command.ExecuteScalarAsync()));

                            // Add the prices related to that product
                            foreach (Price price in product.GetPrices())
                            {
                                SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction);

                                priceCommand.Parameters.AddWithValue("@ProductId", product.Id);
                                priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);

                                price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                            }
                            addedProducts.Add(product);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                        transaction.Commit();
                    }
                }
            }
            return addedProducts;
        }
    }
}