using DAL.Data.DAO;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
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
        public async Task<IActionResult> AddAvis(Avis avis, int companyId)
        {
            try
            {
                int avisId = await _avisDAO.Add(avis, companyId);
                avis.SetId(avisId);
                return Created($"{baseURI}/{avis.Id}", avis);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}