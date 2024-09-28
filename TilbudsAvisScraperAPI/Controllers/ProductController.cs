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

        public async Task<List<Product>> AddProducts(Avis avis)
        {
            return await _productDAO.AddProducts(avis.GetProducts(), avis.Id);
        }
    }
}