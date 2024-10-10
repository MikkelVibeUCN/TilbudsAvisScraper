using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class APIUserController : ControllerBase
    {
        const string baseURI = "api/v1/[controller]";
        private readonly APIUserService _apiUserService;


        public APIUserController(APIUserService apiUserService)
        {
            this._apiUserService = apiUserService;
        }


        [HttpGet]
        public async Task<IActionResult> IsTokenValid(string token, int permissionLevel)
        {
            if(await _apiUserService.IsTokenValid(token, permissionLevel))
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
