using TilbudsAvisLibrary.Entities;
using DAL.Data.Interfaces;

namespace DAL.Data.DAO
{
    public class CompanyDAO : ICompanyDAO
    {
        public Task<int> Add(Company t)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Company?> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Company>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Company t)
        {
            throw new NotImplementedException();
        }
    }
}