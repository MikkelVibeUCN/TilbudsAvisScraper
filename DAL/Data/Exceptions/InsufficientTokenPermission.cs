namespace DAL.Data.Exceptions
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