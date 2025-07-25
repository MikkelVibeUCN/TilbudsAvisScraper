﻿using APIIntegrationLibrary.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;

namespace APIIntegrationLibrary.Client
{
    public class AvisAPIRestClient : BaseClient<AvisDTO>, IAvisAPIRestClient
    {
        public AvisAPIRestClient(string uri, string token) : base(uri, "Avis", token) { }
        public async Task<int> CreateAsync(AvisDTO avis, int companyId, string token)
        {
            return await CreateAsync(avis, $"{_defaultEndPoint}?companyId={companyId}");
        }

        public async Task<AvisDTO?> GetValidAsync(int companyId, string token)
        {
            return await GetAsync(null, $"{_defaultEndPoint}/LatestAvis?companyId={companyId}");
        }
    }
}
