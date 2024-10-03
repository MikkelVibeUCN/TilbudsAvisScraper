using System.Runtime.CompilerServices;
using TilbudsAvisLibrary.Entities;
namespace DAL.Data.Interfaces
{
    public interface IAvisDAO : IDAO<Avis>
    {
        Task<int> Add(Avis avis, int companyId, int permissionLevel);
    }
}
