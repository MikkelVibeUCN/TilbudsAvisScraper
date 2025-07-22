using System;
using System.Threading.Tasks;
using APIIntegrationLibrary.Client;
using Microsoft.Extensions.Options;
using ScraperLibrary.Exceptions;
using Hangfire;

namespace Scraper
{
    public class JobRunner
    {
        private readonly ScraperSettings _config;

        public JobRunner(IOptions<ScraperSettings> options)
        {
            _config = options.Value;
        }

        [DisableConcurrentExecution(3600)] // 1-hour lock
        public async Task ExecuteScrape(int companyId)
        {
            if (string.IsNullOrWhiteSpace(_config.Token) || string.IsNullOrWhiteSpace(_config.ApiUri))
                throw new Exception("ScraperSettings not properly configured.");

            var avis = await Operations.ScrapeAvis(companyId);

            if (avis.Products.Count < 10)
                throw new Exception($"Too few products scraped for company {companyId}");

            var client = new AvisAPIRestClient(_config.ApiUri, _config.Token);
            await client.CreateAsync(avis, companyId, _config.Token);

            Console.WriteLine($"Scrape and save completed for company {companyId}");

            var nextTime = avis.ValidTo.AddDays(1).AddHours(2);

            ScheduleNextScrape(companyId, nextTime);
        }

        private void ScheduleNextScrape(int companyId, DateTime nextTime)
        {
            Console.WriteLine($"Scheduling next scrape for company {companyId} at {nextTime}");

            BackgroundJob.Schedule<JobRunner>(
                x => x.ExecuteScrape(companyId),
                nextTime - DateTime.Now
            );
        }
    }
}
