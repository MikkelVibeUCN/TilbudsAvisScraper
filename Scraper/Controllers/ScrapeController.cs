using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Scraper
{
    [ApiController]
    [Route("scrape")]
    public class ScraperController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ScraperController(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpPost("{companyId}")]
        public IActionResult EnqueueScrape(int companyId)
        {
            _backgroundJobClient.Enqueue<JobRunner>(runner =>
                runner.ExecuteScrape(companyId));

            return Ok($"Scrape for company {companyId} has been enqueued.");
        }

        [HttpPost("all")]
        public IActionResult EnqueueAllScrapes()
        {
            _backgroundJobClient.Enqueue<ScraperBootstrapper>(runner =>
                runner.InitializeScrapers());

            return Ok("All scrapes have been enqueued.");
        }
    }
}
