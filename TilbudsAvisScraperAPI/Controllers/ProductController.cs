using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Controllers
{
    public class ProductController
    {
        private readonly IProductDAO _productDAO;

        public ProductController(IProductDAO productDAO)
        {
            this._productDAO = productDAO;
        }
    }
}