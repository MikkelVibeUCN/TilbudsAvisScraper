using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class APIUserController : ControllerBase
    {
        const string baseURI = "v1/[controller]";
        private readonly APIUserService _apiUserService;


        public APIUserController(APIUserService apiUserService)
        {
            this._apiUserService = apiUserService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IsTokenValid([FromHeader(Name = "Authorization")] string authorization,[FromQuery] int permissionLevel)
        {
            Console.WriteLine("Recieved request");
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                Console.WriteLine("missing stuff");
                return Unauthorized("Missing or invalid Authorization header");
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            Console.WriteLine($"Authorization: {token}");


            if (await _apiUserService.IsTokenValid(token, permissionLevel))
            {
                return Ok();
            }
            else
            {
                Console.WriteLine("bad");
                return Unauthorized("Token provided is not valid for this action");
            }
        }
    }
}
