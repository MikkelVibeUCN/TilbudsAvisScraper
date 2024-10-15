using DAL.Data.DAO;
using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvisController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private readonly IAvisDAO _avisDAO;
        private readonly APIUserService _apiUserService;

        public AvisController(IAvisDAO avisDAO, APIUserService apiUserService)
        {
            this._avisDAO = avisDAO;
            this._apiUserService = apiUserService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAvis([FromBody] Avis avis, [FromQuery] int companyId, [FromQuery] string token)
        {
            int permissionLevelRequired = 2;
            try
            {
                if(await _apiUserService.IsTokenValid(token, permissionLevelRequired))
                {
                    int avisId = await _avisDAO.Add(avis, companyId);
                    avis.SetId(avisId);
                    return Created($"{baseURI}/{avis.Id}", avis);
                }
                else
                {
                    return Unauthorized("Token provided is not valid for this action");
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
            int permissionLevelRequired = 1;
            if (await _apiUserService.IsTokenValid(token, permissionLevelRequired))
            {
                return Ok();
            }
            else
            {
                return Unauthorized("Token provided is not valid for this action");
            }
        }
    }
}