using DAL.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TIlbudsAvisScraperAPI.Services
{
    public class APIUserService
    {
        public readonly IAPIUserDAO _apiUserDAO;
        public APIUserService(IAPIUserDAO aPIUserDAO)
        {
            this._apiUserDAO = aPIUserDAO;
        }
        // Returns -1 if token is invalid
        public async Task<int> GetPermissionLevel(string token)
        {
            return await _apiUserDAO.GetPermissionLevel(token);
        }

        public async Task<bool> IsTokenValid(string token, int permissionLevelRequired)
        {
            int permissionLevel = await GetPermissionLevel(token);
            return permissionLevel >= permissionLevelRequired;
        }
    }
}
