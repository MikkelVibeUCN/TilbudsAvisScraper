using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisScraperAPI.Services;

namespace TilbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ProductsController : ControllerBase
    {

        const string baseURI = "v1/[controller]";
        private readonly APIUserService _apiUserService;
        private readonly ProductService _productService;

        public ProductsController(ProductService productService, APIUserService apiUserService)
        {
            this._productService = productService;
            this._apiUserService = apiUserService;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            try
            {
                var productDTO = await _productService.GetProductAsync(id);
                return Ok(productDTO);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts([FromQuery] ProductQueryParameters parameters)
        {
            try
            {
                var productDTOs = await _productService.GetProductsAsync(parameters);

                return Ok(productDTOs);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult> GetProductCount([FromQuery] ProductQueryParameters parameters)
        {
            try
            {
                var productCount = await _productService.GetProductCountAsync(parameters);
                return Ok(productCount);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("retailers")]
        public async Task<ActionResult> GetValidCompanyNamesFromProductSerach([FromQuery] ProductQueryParameters parameters)
        {
            try
            {
                var companyNames = await _productService.GetValidCompanyNamesFromProductSerach(parameters);
                return Ok(companyNames);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}