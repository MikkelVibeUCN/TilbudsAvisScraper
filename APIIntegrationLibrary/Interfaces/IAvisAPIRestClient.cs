using APIIntegrationLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary.Interfaces
{
    public interface IAvisAPIRestClient
    {
        Task<int> CreateAsync(AvisDTO avis, int companyId, string token);
    }
}
