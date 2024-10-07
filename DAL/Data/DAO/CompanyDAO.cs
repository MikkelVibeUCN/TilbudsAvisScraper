using TilbudsAvisLibrary.Entities;
using DAL.Data.Interfaces;

namespace DAL.Data.DAO
{
    public class CompanyDAO : ICompanyDAO
    {
        public Task<int> Add(Company t, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<Company?> Get(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<List<Company>> GetAll(int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Company t, int permissionLevel)
        {
            throw new NotImplementedException();
        }
    }
}