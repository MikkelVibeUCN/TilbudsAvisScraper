using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
namespace DAL.Data.Interfaces
{
    public interface IAvisDAO
    {
        Task<int> Add(Avis avis, int companyId);
        Task<bool> Delete(int id);
        Task Get(int id);
    }
}
