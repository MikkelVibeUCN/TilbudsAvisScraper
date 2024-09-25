using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary.Entities;

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
            var productId = await _productDAO.Add(product);
            // Assuming there's a method to set the Id internally
            product.SetId(productId);
            return Created($"{baseURI}/{product.Id}", product);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}