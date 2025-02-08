using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.DTO
{
    public class CompanyScheduleDTO
    {
        public int Id { get; set; }           // Company ID matching scraper key
        public string Name { get; set; }      // Company name
        public bool IsActive { get; set; }    // Whether to schedule scrapes
        public DateTime ValidTo { get; set; } // From last successful scrape
        public DateTime NextExpectedRelease { get; set; } // Calculated (ValidTo + buffer)
        public int RetryCount { get; set; }   // Current retry attempt (0 = success)
        public string LastError { get; set; } // Last error message
    }
}
