namespace DAL.Data.Interfaces
{
    public interface IDAO<T>
    {
        Task<T?> Get(int id);
        Task<List<T>> GetAll();
        Task<int> Add(T t);
        Task<bool> Update(T t);
        Task Delete(int id);
    }
}