using TilbudsAvisLibrary.DTO;
using DAL.Data.DAO;
using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisScraperAPI.Services;
using TilbudsAvisScraperAPI.Tools;
using Amazon.SQS.Model;
using System.Diagnostics;

namespace TilbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AvisController : ControllerBase
    {
        private readonly AvisService _avisService;
        private readonly APIUserService _apiUserService;

        public AvisController(AvisService avisService, APIUserService apiUserService)
        {
            this._avisService = avisService;
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
                    int avisId = await _avisService.Add(avis, companyId);

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

        [HttpGet("LatestAvis")]
        public async Task<IActionResult> Get([FromHeader(Name = "Authorization")] string authorization, [FromQuery] int companyId)
        {
            int permissionLevelRequired = 3;

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid Authorization header");
            }

            var token = authorization.Substring("Bearer ".Length).Trim();

            if (await _apiUserService.IsTokenValid(token, permissionLevelRequired))
            {
                try
                {
                    AvisDTO avisDTO = await _avisService.GetLatestAvisForCompany(companyId);

                    return Ok(avisDTO);
                }
                catch(Exception ex)
                {
                    return NotFound($"No avis found for companyId: {companyId}, message: {ex.Message}");
                }
            }
            else
            {
                return Unauthorized("Invalid token");
            }
        }
    }
}