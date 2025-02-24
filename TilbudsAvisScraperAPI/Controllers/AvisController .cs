using TilbudsAvisLibrary.DTO;
using DAL.Data.DAO;
using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Services;
using TIlbudsAvisScraperAPI.Tools;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvisController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private readonly AvisService _avisService;
        private readonly APIUserService _apiUserService;

        public AvisController(AvisService avisService, APIUserService apiUserService)
        {
            this._avisService = avisService;
            this._apiUserService = apiUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AvisDTO avis, [FromQuery] int companyId, [FromQuery] string token)
        {
            int permissionLevelRequired = 2;
            try
            {
                if(await _apiUserService.IsTokenValid(token, permissionLevelRequired))
                {
                    int avisId = await _avisService.Add(avis, companyId);

                    return Ok(avisId);
                }
                else
                {
                    return Unauthorized("Invalid token");
                }
            }
            catch (Exception e)
            { 
                return Conflict(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int companyId, string token)
        {
            int permissionLevelRequired = 1;
            if (await _apiUserService.IsTokenValid(token, permissionLevelRequired))
            {


                return Ok();
            }
            else
            {
                return Unauthorized("Invalid token");
            }
        }
    }
}