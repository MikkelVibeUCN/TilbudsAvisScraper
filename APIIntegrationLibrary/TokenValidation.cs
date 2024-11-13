using System.Net.Http;

namespace APIIntegrationLibrary
{
    public class TokenValidation : HClient
    {
        public async Task<bool> VerifyToken(string token, int permissionLevel)
        {
            try
            {
                // Call the API to validate the token
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:5001/api/v1/APIUser?token={token}&permissionLevel={permissionLevel}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return false;
                }
                else
                {
                    throw new Exception($"An error occurred: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred");
            }
        }
    }
}
