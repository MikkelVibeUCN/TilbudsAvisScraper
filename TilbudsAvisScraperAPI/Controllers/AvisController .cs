using DAL.Data.DAO;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvisController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private ProductController _productController;
        private readonly IAvisDAO _avisDAO;

        public AvisController(IAvisDAO avisDAO)
        {
            this._avisDAO = avisDAO;
            this._productController = new ProductController(new ProductDAO());
        }

        [HttpPost]
        public async Task<IActionResult> AddAvis(Avis avis, int companyId)
        {
            var avisId = await _avisDAO.Add(avis, companyId);
            avis.SetId(avisId);
            await _productController.AddProducts(avis);
            return Created($"{baseURI}/{avis.Id}", avis);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}