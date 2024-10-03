using TilbudsAvisLibrary.Entities;
using System.Data.SqlClient;
using DAL.Data.Interfaces;
using System.Diagnostics;

namespace DAL.Data.DAO
{
    public class ProductDAO : DAObject, IProductDAO
    {
        private string productQuery = "INSERT INTO Product (ExternalId, Name, Description, ImageUrl) " +
                       "VALUES (@ExternalId, @Name, @Description, @ImageUrl); " +
                       "SELECT SCOPE_IDENTITY();";

        private string priceQuery = "INSERT INTO Price (ProductId, Price, AvisId) " +
                   "VALUES (@ProductId, @Price, @AvisId)" +
                    "";

        public ProductDAO()
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                Console.WriteLine("Connection opened");

            }
            Console.WriteLine("Connection closed");
        }

        public Task Delete(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public async Task<Product?> GetProductFromExernalId(int inputExternalId)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    SqlCommand command = new SqlCommand("SELECT * FROM Product WHERE ExternalId = @ExternalId", connection);
                    command.Parameters.AddWithValue("@ExternalId", inputExternalId);

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();

                        int productId = reader.GetInt32(reader.GetOrdinal("Id"));
                        string name = reader.GetString(reader.GetOrdinal("Name"));
                        string description = reader.GetString(reader.GetOrdinal("Description"));
                        string imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
                        int externalId = reader.GetInt32(reader.GetOrdinal("ExternalId"));

                        List<Price> prices = await GetPricesForProduct(productId);

                        Product product = new Product(name, imageUrl, description, externalId, productId, prices);

                        return product;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    throw ex;
                }
            }
        }

        public async Task<Product?> Get(int id, int permissionLevel)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    SqlCommand command = new SqlCommand("SELECT * FROM Product WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();

                        int productId = reader.GetInt32(reader.GetOrdinal("Id"));
                        string name = reader.GetString(reader.GetOrdinal("Name"));
                        string description = reader.GetString(reader.GetOrdinal("Description"));
                        string imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
                        int externalId = reader.GetInt32(reader.GetOrdinal("ExternalId"));

                        List<Price> prices = await GetPricesForProduct(productId);

                        Product product = new Product(name, imageUrl, description, externalId, productId, prices);

                        return product;
                    }
                    else
                    {
                        return null;
                    }
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

                string query = "SELECT * FROM Price WHERE ProductId = @ProductId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            float price = (float)reader.GetDouble(reader.GetOrdinal("Price"));
                            int avidId = reader.GetInt32(reader.GetOrdinal("AvisId"));

                            Price newPrice = new Price(id, price, avidId);
                            prices.Add(newPrice);
                        }
                    }
                }
            }
            return prices;
        }

        public Task<List<Product>> GetAll(int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task Update(Product product, int permissionLevel)
        {
            throw new NotImplementedException();
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
                        SqlCommand command = new SqlCommand(productQuery, connection, transaction);

                        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl);

                        product.SetId(Convert.ToInt32(await command.ExecuteScalarAsync()));

                        // Add the prices related to that product
                        await CreatePrices(avis.Id, connection, transaction, product);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    await CreatePrices(avis.Id, connection, transaction, product);

                    product.SetId((int)checkProduct.Id);
                }
                addedProducts.Add(product);
            }
            return addedProducts;
        }

        private async Task<bool> PriceAlreadyExists(Price price, int productId)
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT a.id FROM Price p JOIN Avis a ON p.AvisId = a.Id WHERE p.ProductId = @ProductId AND a.ExternalId = @ExternalId;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@ExternalId", price.ExternalAvisId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        private async Task CreatePrices(int avisId, SqlConnection connection, SqlTransaction transaction, Product product)
        {
            foreach (Price price in product.GetPrices())
            {
                if (!await PriceAlreadyExists(price, (int)product.Id))
                {
                    SqlCommand priceCommand = new SqlCommand(priceQuery, connection, transaction);

                    priceCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);
                    priceCommand.Parameters.AddWithValue("@AvisId", avisId);

                    price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                }
            }
        }
    }
}