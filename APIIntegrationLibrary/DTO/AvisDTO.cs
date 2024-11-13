using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace APIIntegrationLibrary.DTO
{
    public class AvisDTO
    {
        //public List<PageDTO> Pages { get|; set; }
        public List<ProductDTO> Products { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int Id { get; set; }
    }
}
