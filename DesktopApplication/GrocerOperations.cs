using APIIntegrationLibrary.DTO;
using ScraperLibrary._365_Discount;
using ScraperLibrary.COOP.Kvickly;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisLibrary.Exceptions;

namespace DesktopApplication
{
    public class GrocerOperations
    {
        public async Task<AvisDTO?> ScrapeRemaAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            try
            {
                return await new RemaAvisScraper().GetAvis(progressCallback, token, companyId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<AvisDTO?> Scrape365Avis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            try
            {
                return await new _365AvisScraper().GetAvis(progressCallback, token, companyId);
            }
            catch (CannotReachWebsiteException e)
            {
                throw e;
            }
            catch
            {
                return null;
            }
        }
        public async Task<AvisDTO?> ScrapeKvicklyAvis(Action<int> progressCallback, CancellationToken token, int companyId)
        {
            try
            {   
                return await new KvicklyAvisScraper().GetAvis(progressCallback, token, companyId);
            }
            catch (CannotReachWebsiteException e)
            {

                throw e;
            }
            catch
            {
                return null;
            }
        }
    }
}