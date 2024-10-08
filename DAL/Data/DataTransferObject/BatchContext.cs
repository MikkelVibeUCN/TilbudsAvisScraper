using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data.DataTransferObject
{
    public class BatchContext
    {
        public int BaseId { get; set; }
        public int Id { get; set; }
        public string ExternalId { get; set; }

        public BatchContext(int baseId = 0, int id = 0, string externalId = "")
        {
            BaseId = baseId;
            Id = id;
            ExternalId = externalId;
        }
    }
}
