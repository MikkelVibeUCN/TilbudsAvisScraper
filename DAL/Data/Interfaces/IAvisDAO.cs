using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
namespace DAL.Data.Interfaces
{
    public interface IAvisDAO
    {
        Task<int> Add(Avis avis, int companyId);
        Task<bool> Delete(int id);
        Task<Avis?> Get(int id);
        Task<int> GetLatestAvisId(int companyId);
    }
}
