using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperLibrary.Exceptions
{
    public class FutureValidFromException : Exception
    {
        public DateTime ValidFrom { get; }

        public FutureValidFromException(DateTime validFrom)
            : base($"Avis start date is in the future: {validFrom:yyyy-MM-dd HH:mm:ss}")
        {
            ValidFrom = validFrom;
        }
    }
}
