using TilbudsAvisLibrary.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary;
using TilbudsAvisWeb.Services;

namespace TilbudsAvisWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }
        public async Task<ActionResult> Index(ProductQueryParameters parameters)
        {
            ViewBag.Grocers = await _productService.GetValidCompanyNamesFromProductSearch(parameters);

            int totalProductsForSearch = await _productService.GetProductCountAsync(parameters);
            int totalPages = (int)Math.Ceiling(totalProductsForSearch / (double)parameters.PageSize) - 1;

            ViewBag.CurrentSortOrder = parameters.SortBy;
            ViewBag.CurrentRetailer = parameters.Retailer;
            ViewBag.CurrentPage = parameters.PageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearchTerm = parameters.SearchTerm;

            IEnumerable<ProductDTO> products = await _productService.GetProductsAsync(parameters);

            return View(products);
        }

        // GET: ProductController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            ProductDTO? productDTO = await _productService.GetProductAsync(id);
            if(productDTO == null)
            {
                return View("NotFound");
            }
            return View(productDTO);
        }
    }
}
