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
        private readonly IAvisDAO _avisDAO;

        public AvisController(IAvisDAO avisDAO)
        {
            this._avisDAO = avisDAO;
        }

        [HttpPost]
        public async Task<IActionResult> AddAvis(Avis avis)
        {
            var avisId = await _avisDAO.Add(avis);
            // Assuming there's a method to set the Id internally
            avis.SetId(avisId);
            return Created($"{baseURI}/{avis.Id}", avis);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}