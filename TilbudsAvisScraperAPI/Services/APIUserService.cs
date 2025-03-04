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
        // Returns 1 if token is not found or invalid since 1 is default permission level
        public async Task<int> GetPermissionLevel(string token)
        {
            return await _apiUserDAO.GetPermissionLevel(token);
        }

        public async Task<bool> IsTokenValid(string token, int permissionLevelRequired)
        {
            return await GetPermissionLevel(token) >= permissionLevelRequired;
        }
    }
}
