using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary.DTO
{
    public class PriceDTO
    {
        public float Price { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string CompareUnit { get; set; }
        public string CompanyName { get; set; }
    }
}
