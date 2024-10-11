using DAL.Data.Batch;
using DAL.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class PriceDAO : DAObject, IPriceDAO
    {
        private readonly string _getPriceAndExternalAvisIdquery = @"SELECT p.*, a.ExternalId FROM Price p JOIN Avis a ON p.AvisId = a.Id WHERE p.ProductId = @ProductId;";

        private readonly string _addPriceQuery = "INSERT INTO Price (ProductId, Price, AvisId, CompareUnitString) " +
                        "VALUES (@ProductId, @Price, @AvisId, @CompareUnitString)";
        public Task<int> Add(Price pricel)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Price?> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Price>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Price pricel)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Price>> GetPricesForProduct(int productId)
        {
            List<Price> prices = new List<Price>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(_getPriceAndExternalAvisIdquery, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            float price = (float)reader.GetDouble(reader.GetOrdinal("Price"));
                            string externalAvisId = reader.GetString(reader.GetOrdinal("ExternalId"));

                            string CompareUnitString = reader.GetString(reader.GetOrdinal("CompareUnitString"));
                            Price newPrice = new Price(id, price, externalAvisId, CompareUnitString);
                            prices.Add(newPrice);
                        }
                    }
                }
            }
            return prices;
        }
        private async Task<List<Product>> AddPricesInBatch(List<Product> products, SqlConnection connection, SqlTransaction transaction, int baseAvisId, int avisId, string avisExternalId)
        {
            return await AddTInBatch(products, connection, transaction, AddPricesInBatchInternal, new BatchContext(baseAvisId, avisId, avisExternalId));
        }

        private async Task<List<Product>> AddPricesInBatchInternal(List<Product> products, SqlConnection connection, SqlTransaction transaction, BatchContext context)
        {
            // Initialize the SQL command text builder
            var commandText = new StringBuilder();
            commandText.AppendLine("INSERT INTO Price (ProductId, Price, AvisId, CompareUnitString)");
            commandText.AppendLine("OUTPUT INSERTED.Id");  // Only get the generated Price IDs
            commandText.AppendLine("VALUES ");

            // Create a list to hold the parameterized rows
            List<string> rows = new();

            // Dictionary to track prices for each product
            Dictionary<int, List<Price>> priceMap = new();

            int paramCounter = 0;  // Counter to handle unique parameter names for multiple prices

            // Loop through each product and its associated prices
            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];

                if (product.Prices != null && product.Prices.Any())
                {
                    foreach (var price in product.Prices)
                    {
                        // Add a parameterized row for each price associated with the product
                        rows.Add($"(@ProductId{paramCounter}, @Price{paramCounter}, @AvisId{paramCounter}, @CompareUnitString{paramCounter})");

                        // Map the product ID to its prices in the dictionary
                        if (!priceMap.ContainsKey(i))
                        {
                            priceMap[i] = new List<Price>();
                        }
                        priceMap[i].Add(price);  // Add the price to this product's list

                        paramCounter++;
                    }
                }
            }

            // If there are rows to insert
            if (rows.Count > 0)
            {
                commandText.AppendLine(string.Join(", ", rows));  // Concatenate the rows into the SQL query
                commandText.AppendLine(";");

                using (SqlCommand command = new SqlCommand(commandText.ToString(), connection, transaction))
                {
                    // Reset paramCounter for adding parameter values
                    paramCounter = 0;

                    // Loop again through the products and prices to add the parameters
                    for (int i = 0; i < products.Count; i++)
                    {
                        var product = products[i];

                        if (product.Prices != null && product.Prices.Any())
                        {
                            foreach (var price in product.Prices)
                            {
                                // Add parameters for each price
                                command.Parameters.AddWithValue($"@ProductId{paramCounter}", product.Id);
                                command.Parameters.AddWithValue($"@Price{paramCounter}", price.PriceValue);

                                // Determine the correct AvisId based on the ExternalAvisId and context
                                if (price.ExternalAvisId.Equals(context.ExternalId))
                                {
                                    command.Parameters.AddWithValue($"@AvisId{paramCounter}", context.BaseId);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue($"@AvisId{paramCounter}", context.Id);
                                }

                                // Add the CompareUnitString parameter
                                command.Parameters.AddWithValue($"@CompareUnitString{paramCounter}", price.CompareUnitString);

                                paramCounter++;
                            }
                        }
                    }

                    // Execute the command and read the inserted Price IDs
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Keep track of product index and price index for proper mapping
                        int productIndex = 0;
                        int priceIndex = 0;

                        while (await reader.ReadAsync())
                        {
                            int insertedPriceId = reader.GetInt32(0);  // Get the generated Price ID

                            // Retrieve the price list for the current product
                            var prices = priceMap[productIndex];

                            // Set the generated Price ID to the current price
                            prices[priceIndex].SetId(insertedPriceId);

                            // Move to the next price in the current product's list
                            priceIndex++;

                            // If we've processed all prices for the current product, move to the next product
                            if (priceIndex >= prices.Count)
                            {
                                productIndex++;
                                priceIndex = 0;
                            }
                        }
                    }
                }
            }

            return products;  // Return the updated list of products with assigned price IDs
        }

        public async Task AddPricesForProduct(Product product, int baseAvisId, int avisId, string avisExternalId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    await AddPricesForProduct(connection, transaction, product, baseAvisId, avisId, avisExternalId);
                    transaction.Commit();
                }
            }
        }

        public async Task AddPricesForProduct(SqlConnection connection, SqlTransaction transaction, Product product, int baseAvisId, int avisId, string avisBaseExternalId)
        {
            foreach (Price price in product.GetPrices())
            {
                string externalIdToUse = price.ExternalAvisId;
                
                using (SqlCommand priceCommand = new SqlCommand(_addPriceQuery, connection, transaction))
                {
                    priceCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    priceCommand.Parameters.AddWithValue("@Price", price.PriceValue);

                    int idToUse = avisId;

                    if (externalIdToUse.Equals(avisBaseExternalId))
                    {
                        idToUse = baseAvisId;
                    }
                    priceCommand.Parameters.AddWithValue("@AvisId", idToUse);

                    priceCommand.Parameters.AddWithValue("@CompareUnitString", price.CompareUnitString);

                    price.SetId(Convert.ToInt32(await priceCommand.ExecuteScalarAsync()));
                }
            }
        }

        public async Task AddPricesForProducts(SqlConnection connection, SqlTransaction transaction, List<Product> products, int baseAvisId, int avisId, string avisExternalId)
        {
            await AddPricesInBatch(products, connection, transaction, baseAvisId, avisId, avisExternalId);
        }

        public async Task AddPricesForProducts(List<Product> products, int baseAvisId, int avisId, string avisExternalId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    await AddPricesForProducts(connection, transaction, products, baseAvisId, avisId, avisExternalId);
                    transaction.Commit();
                }
            }

        }
    }
}
