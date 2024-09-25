using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Dao.Interfaces;
using TIlbudsAvisScraperAPI.Data.Interfaces;

namespace TIlbudsAvisScraperAPI.Data.DAO
{
    public class AvisDAO : IAvisDAO
    {
        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Avis> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Avis>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task Update(Avis avis)
        {
            throw new NotImplementedException();
        }

        Task<int> IDAO<Avis>.Add(Avis t)
        {
            throw new NotImplementedException();
        }
    }
}