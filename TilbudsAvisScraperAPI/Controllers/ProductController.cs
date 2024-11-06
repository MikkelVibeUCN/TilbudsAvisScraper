using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary;
using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductController
    {

        const string baseURI = "api/v1/[controller]";
        private readonly APIUserService _apiUserService;
        private readonly IProductDAO _productDAO;

        public ProductController(IProductDAO productDAO, APIUserService apiUserService)
        {
            this._productDAO = productDAO;
            this._apiUserService = apiUserService;
        }



        [HttpGet]
        public async Task<List<Product>> GetProducts(ProductQueryParameters parameters)
        {
            IQueryable<Product> queryable = _productDAO.GetProducts();
        }




    }
}