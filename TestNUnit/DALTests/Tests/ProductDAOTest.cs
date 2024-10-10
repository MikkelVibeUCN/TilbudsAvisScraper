using DAL.Data.DAO;
using DAL.Data.Interfaces;
using TilbudsAvisLibrary.Entities;

namespace TestNUnit.DALTests.Tests
{
    public class ProductDAOTest
    {
        List<Product> productsTestList = new();
        int productAmount = 1000;
        string avisExternalId = "abcdef";
        string avisBaseExternalId = "testBase";

        IProductDAO _productDAO;
        IAvisDAO _avisDAO;

        int baseAvisId;
        int avisId;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            try
            {
                _productDAO = new ProductDAO(new NutritionInfoDAO(), new PriceDAO());
                _avisDAO = new AvisDAO(_productDAO);

                for (int i = 0; i < productAmount; i++)
                {
                    List<Price> pricesInProduct = new();
                    for (int j = 0; j < 2; j++)
                    {
                        string compareUnitString = "kg";
                        if (i < productAmount / 2)
                        {
                            compareUnitString = "stk";
                        }
                        Price price;
                        if (j == 0)
                        {
                            price = new Price(i, avisBaseExternalId, compareUnitString);
                        }
                        else
                        {
                            price = new Price(i, avisExternalId, compareUnitString);
                        }
                        pricesInProduct.Add(price);
                    }

                    NutritionInfo? nutritionInfo = null;
                    if (i < productAmount / 2)
                    {
                        nutritionInfo = new(100, 20, 20, 20, 20, 20, 20);
                    }
                    Product product = new Product(pricesInProduct, null, "Cool name", "url", "cool product", -i - 10, nutritionInfo, 5);
                    productsTestList.Add(product);
                }
                Avis baseAvis = new(avisBaseExternalId, DateTime.Now, DateTime.Now, new());
                Avis avis = new(avisExternalId, DateTime.Now, DateTime.Now, new());

                baseAvisId = await _avisDAO.Add(baseAvis, 1);
                avisId = await _avisDAO.Add(avis, 1);

                baseAvis.SetId(baseAvisId);
                avis.SetId(avisId);

            }
            catch
            {
                Assert.Fail("Setup failed");
            }
        }

        [Test]
        public async Task AddingProductsInBulkTest()
        {
            try
            {
                List<Product> productsWithIds = await _productDAO.AddProducts(productsTestList, baseAvisId, avisId, avisBaseExternalId);

                foreach (var product in productsWithIds)
                {
                    if(product.Id == null)
                    {
                        Assert.Fail("Product ID is null");
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [Test]
        public async Task AddingSingleProductTest()
        {
            int productId = 0;
            try
            {
                List<Price> prices = new List<Price>();
                prices.Add(new Price(20, avisBaseExternalId, "kg"));
                prices.Add(new Price(30, avisExternalId, "kg"));

                Product product = new(prices, null, "swag", "url", "sej produkt", -5000, new NutritionInfo(20, 20, 20, 20, 20, 20, 20), 5);
                productId = await _productDAO.Add(product, 3, baseAvisId, avisId, avisBaseExternalId);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
            Assert.That(productId != 0);
        }

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                await _productDAO.DeleteNegativeExternalIds();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            try
            {
                await _productDAO.DeleteNegativeExternalIds();

                await _avisDAO.Delete(avisId);
                await _avisDAO.Delete(baseAvisId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}