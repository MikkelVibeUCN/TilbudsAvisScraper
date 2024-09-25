using System.Data.SqlClient;
namespace TIlbudsAvisScraperAPI.Data.Interfaces
{
    public interface IDAO<T>
    {

        Task<T> Get(int id);
        Task<List<T>> GetAll();
        Task<int> Add(T t);
        Task Update(T t);
        Task Delete(int id);
    }
}