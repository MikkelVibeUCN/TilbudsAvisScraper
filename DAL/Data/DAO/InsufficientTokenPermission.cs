namespace DAL.Data.DAO
{
    [Serializable]
    public class InsufficientTokenPermission : Exception
    {
        public InsufficientTokenPermission(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public InsufficientTokenPermission(string? message) : base(message)
        {
        }
    }
}