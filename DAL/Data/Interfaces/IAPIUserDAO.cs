using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.Interfaces
{
    public interface IAPIUserDAO : IDAO<APIUser>
    {
        public Task<int> GetPermissionLevel(string token);
    }
}
