namespace DAL.Data.Exceptions
{
    [Serializable]
    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public AlreadyExistsException(string? message) : base(message)
        {
        }
    }
}