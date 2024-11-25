using APIIntegrationLibrary.Client;
using TilbudsAvisLibrary.DTO;
using DAL.Data.DAO;
using DAL.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestNUnit.APIRestClientTests
{
    public class AvisAPIClientTests
    {
        private static Random random = new Random();

        private string _Token = new ConfigurationBuilder()
            .AddUserSecrets<AvisAPIClientTests>()
            .Build()
            .GetSection("SecretToken")
            .Value;
        private const int _testCompanyId = 6;

        private int _currentAvisId = 0;
        private static readonly ProductDAO productDAO = new ProductDAO(new NutritionInfoDAO(), new PriceDAO());
        private static  readonly AvisDAO avisDAO = new AvisDAO(productDAO);


        private readonly AvisAPIRestClient _avisAPIRestClient = new AvisAPIRestClient("https://localhost:7133/api/v1/");
        public void Setup()
        {
            // Existing setup code
        }

        private AvisDTO CreateRandomAvis()
        {
            var products = new List<ProductDTO>();
            for (int i = 0; i < 200; i++)
            {
                var product = new ProductDTO
                {
                    Description = "Random food " + i,
                    Name = "Cool name",
                    ImageUrl = "http://random.com",
                    NutritionInfo = new NutritionInfoDTO
                    {
                        CarbohydratesPer100G = random.Next(1, 100),
                        FiberPer100G = random.Next(1, 100),
                        EnergyKJ = random.Next(1, 100),
                        FatPer100G = random.Next(1, 100),
                        ProteinPer100G = random.Next(1, 100),
                        SaltPer100G = random.Next(1, 100),
                        SaturatedFatPer100G = random.Next(1, 100),
                        SugarsPer100G = random.Next(1, 100),
                    },
                    Prices = new List<PriceDTO>(),
                    ExternalId = (-((1+i)*20000)).ToString(),
                    Amount = random.Next(1, 100)
                };

                for (int j = 1; j == 1; j++)
                {
                    product.Prices.Add(new PriceDTO
                    {
                        CompanyName = "test",
                        CompareUnit = "kg",
                        Price = random.Next(1, 100),
                        ValidFrom = DateTime.Now,
                        ValidTo = DateTime.Now.AddDays(random.Next(1, 30))
                    });
                }
                products.Add(product);
            }

            return new AvisDTO
            {
                ValidTo = DateTime.Now.AddDays(random.Next(1, 30)),
                ValidFrom = DateTime.Now,
                ExternalId = "test",
                Products = products
            };
        }

        [Test]
        public async Task CreateAvisWithValidPermissions()
        {
            AvisDTO avisTest = CreateRandomAvis();
            _currentAvisId = await _avisAPIRestClient.CreateAsync(avisTest, _testCompanyId, _Token);

            Assert.That(_currentAvisId > 0);

        }

        [TearDown]
        public async Task TearDown()
        {
            await productDAO.DeleteTestProducts();

            await avisDAO.Delete(_currentAvisId);
        }
    }
}
