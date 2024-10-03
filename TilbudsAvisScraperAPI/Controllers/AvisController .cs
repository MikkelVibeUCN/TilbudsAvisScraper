using DAL.Data.DAO;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvisController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private readonly IAvisDAO _avisDAO;
        private readonly IAPIUserDAO _apiUserDAO;

        public AvisController(IAvisDAO avisDAO, IAPIUserDAO apiUserDAO)
        {
            this._avisDAO = avisDAO;
            this._apiUserDAO = apiUserDAO;
        }

        [HttpPost]
        public async Task<IActionResult> AddAvis(Avis avis, int companyId, string token)
        {
            try
            {
                if(await _apiUserDAO.GetPermissionLevel(token) >= 2)
                {
                    int avisId = await _avisDAO.Add(avis, companyId);
                    avis.SetId(avisId);
                    return Created($"{baseURI}/{avis.Id}", avis);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string token)
        {
            if(await _apiUserDAO.GetPermissionLevel(token) >= 1)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}