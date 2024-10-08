using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
namespace DAL.Data.Interfaces
{
    public interface IAvisDAO
    {
        Task<int> Add(Avis avis, int companyId, int permissionLevel);
        Task<bool> Delete(int id, int permissionLevel);
        Task Get(int id, int permissionLevel);
    }
}
