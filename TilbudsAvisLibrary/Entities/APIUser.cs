using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Entities
{
    public class APIUser
    {
        public int? Id { get; private set; }
        public string Role { get; private set; }
        public int PermissionLevel { get; private set; }
        public string Token { get; private set; }

        public APIUser(string role, int permissionLevel, int? id)
        {
            Id = id;
            Role = role;
            PermissionLevel = permissionLevel;
            Token = GenerateUniqueToken();
        }

        public static string GenerateUniqueToken()
        {
            string guidPart = Guid.NewGuid().ToString("N");

            // Generate random bytes and convert them to a hexadecimal string
            byte[] randomBytes = new byte[8]; 
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            string randomPart = BitConverter.ToString(randomBytes).Replace("-", "");

            // Get the current timestamp in ticks (for uniqueness across time)
            string timestampPart = DateTime.UtcNow.Ticks.ToString();

            // Combine all parts: GUID + Random Bytes + Timestamp
            return $"{guidPart}{randomPart}{timestampPart}";
        }
        public void SetId(int id) => Id = id;
    }
}