using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Exceptions
{
    [Serializable]
    public class CannotReachWebsiteException : Exception
    {
        public CannotReachWebsiteException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public CannotReachWebsiteException(string? message) : base(message)
        {
        }
    }
}
