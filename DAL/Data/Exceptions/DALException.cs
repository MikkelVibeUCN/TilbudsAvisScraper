namespace DAL.Data.Exceptions
{
    [Serializable]
    public class DALException : Exception
    {
        public DALException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public DALException(string? message) : base(message)
        {
        }
    }
}