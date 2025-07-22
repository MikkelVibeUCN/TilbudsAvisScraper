using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Options;
using APIIntegrationLibrary.Client;
using TilbudsAvisLibrary.DTO;

namespace Scraper
{
    public class ScraperBootstrapper
    {
        private readonly ScraperSettings _config;
        private readonly AvisAPIRestClient _client;

        public ScraperBootstrapper(IOptions<ScraperSettings> options)
        {
            _config = options.Value ?? throw new Exception("Missing scraper config");
            _client = new AvisAPIRestClient(_config.ApiUri, _config.Token);
        }

        public async Task InitializeScrapers()
        {
            var scrapers = Operations.GetScrapers();
            foreach (var companyId in scrapers.Keys)
            {
                DateTime nextTime;

                try
                {
                    var latestAvis = await _client.GetValidAsync(companyId, _config.Token);

                    nextTime = latestAvis != null
                        ? latestAvis.ValidTo.AddDays(1).AddHours(2)
                        : DateTime.Now.AddMinutes(1);
                }
                catch
                {
                    // Fallback: try again soon
                    nextTime = DateTime.Now.AddMinutes(5);
                }

                BackgroundJob.Schedule<JobRunner>(
                    runner => runner.ExecuteScrape(companyId),
                    nextTime - DateTime.Now
                );
            }
        }
    }
}
