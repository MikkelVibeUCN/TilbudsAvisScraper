using DAL.Data.DAO;
using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace TestNUnit.DALTests.Tests
{
    public class ProductDAOTest
    {
        List<Product> productsTestList = new();
        int productAmount = 1000;
        string avisExternalId = "abcdef";
        string avisBaseExternalId = "testBase";

        IProductDAO _productDAO = new ProductDAO();

        IAvisDAO _avisDAO;

        Avis baseAvis;
        Avis avis;

        [OneTimeSetUp]
        public void Setup()
        {
            _avisDAO = new AvisDAO(_productDAO);

            for (int i = 0; i < productAmount; i++)
            {
                List<Price> pricesInProduct = new();
                for (int j = 0; j < 5; j++)
                {
                    string compareUnitString = "kg";
                    Price price;
                    if (j == 0)
                    {
                        price = new Price(i, avisBaseExternalId, "kg");
                    }
                    else
                    {
                        price = new Price(i, avisExternalId, "kg");
                    }
                    pricesInProduct.Add(price);
                }

                NutritionInfo? nutritionInfo = null;
                if (i < productAmount / 2)
                {
                    nutritionInfo = new(100, 20, 20, 20, 20, 20, 20);
                }
                Product product = new Product(pricesInProduct, null, "Cool name", "url", "cool product", i, nutritionInfo, 5);
                productsTestList.Add(product);
            }
            baseAvis = new(avisBaseExternalId, DateTime.MinValue, DateTime.MaxValue, new());
            avis = new(avisExternalId, DateTime.MinValue, DateTime.MaxValue, new());
        }

        [Test]
        public async Task AddingProductsInBulkTest()
        {
            try
            {
                int baseAvisId = await _avisDAO.Add(baseAvis, 1, 3);
                int avisId = await _avisDAO.Add(avis, 1, 3);
                baseAvis.SetId(baseAvisId);
                avis.SetId(avisId);
                
                await _productDAO.AddProductsInBatch(productsTestList, baseAvisId, avisId);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _avisDAO.Delete(baseAvis.Id, 3);
            await _avisDAO.Delete(avis.Id, 3);
        }
    }
}
