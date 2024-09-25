using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Dao;
using TIlbudsAvisScraperAPI.Dao.Interfaces;
using TIlbudsAvisScraperAPI.Data.Interfaces;

namespace TIlbudsAvisScraperAPI.Data.DAO
{
    public class CompanyDAO : ICompanyDAO
    {
        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Company> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Company>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task Update(Company company)
        {
            throw new NotImplementedException();
        }

        Task<int> IDAO<Company>.Add(Company company)
        {
            throw new NotImplementedException();
        }
    }
}