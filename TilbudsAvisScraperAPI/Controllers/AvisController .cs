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
using Amazon.SQS.Model;
using System.Diagnostics;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AvisController : ControllerBase
    {
        private readonly IAvisDAO _avisDAO;
        private readonly APIUserService _apiUserService;

        public AvisController(IAvisDAO avisDAO, APIUserService apiUserService)
        {
            this._avisDAO = avisDAO;
            this._apiUserService = apiUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AvisDTO avis, [FromQuery] int companyId, [FromHeader(Name = "Authorization")] string authorization)
        {
            int permissionLevelRequired = 2;

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid Authorization header");
            }

            var token = authorization.Substring("Bearer ".Length).Trim();

            try
            {
                if(await _apiUserService.IsTokenValid(token, permissionLevelRequired))
                {

                    Avis mappedAvis = EntityMapper.MapToEntity(avis);

                    int avisId = await _avisDAO.Add(mappedAvis, companyId);

                    if (avisId == 0)
                    {
                        return Conflict("Avis could not be created");
                    }

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
        public async Task<IActionResult> Get([FromHeader(Name = "Authorization")] string authorization)
        {
            int permissionLevelRequired = 1;

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid Authorization header");
            }

            var token = authorization.Substring("Bearer ".Length).Trim();

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