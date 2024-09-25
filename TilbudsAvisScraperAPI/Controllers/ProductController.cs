using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Dao;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private readonly IProductDAO _productDAO;

        public ProductController(IProductDAO productDAO)
        {
            this._productDAO = productDAO;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            product.Id = await _productDAO.Add(product);
            return Created($"{baseURI}/{product.Id}", product);
        }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }



    }
}