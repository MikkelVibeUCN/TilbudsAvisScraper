namespace DAL.Data.Interfaces
{
    public interface IDAO<T>
    {
        Task<T?> Get(int id, int permissionLevel);
        Task<List<T>> GetAll(int permissionLevel);
        Task<int> Add(T t, int permissionLevel);
        Task<bool> Update(T t, int permissionLevel);
        Task Delete(int id, int permissionLevel);
    }
}