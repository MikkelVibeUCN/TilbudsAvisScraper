using System.Diagnostics;
using System.Reflection.Metadata;
using APIIntegrationLibrary.Client;
using ScraperLibrary._365_Discount;
using ScraperLibrary.Interfaces;
using TilbudsAvisLibrary.DTO;


namespace AutomaticScraperConsoleApp
{
    class Program
    {
        private static string URI;
        private static string TOKEN;
        private static readonly Dictionary<int, System.Timers.Timer> _schedules = new Dictionary<int, System.Timers.Timer>();
        private static AvisAPIRestClient _avisAPIRestClient;
        static async Task Main(string[] args)
        {
            TOKEN = args[0];
            URI = args[1];
            if (TOKEN == null && URI == null) { return; }
            _avisAPIRestClient = new AvisAPIRestClient(URI, TOKEN);
            Console.WriteLine("Created RestClient with token");
            Console.WriteLine("Starting Automatic Scraper Console App...");
            try
            {
                await ScheduleAllScrapers();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        private static async Task ScheduleAllScrapers()
        {
            var companyIds = Operations.GetScrapers().Keys;

            foreach (var companyId in companyIds)
            {
                var latestAvis = await _avisAPIRestClient.GetValidAsync(companyId, TOKEN);

                await ScheduleNextScrape(companyId, latestAvis.ValidTo);
            }
        }

        private static async Task ScheduleNextScrape(int companyId, DateTime expiryDate)
        {

            var timeUntilExpiry = expiryDate - DateTime.Now;

            if (timeUntilExpiry.TotalMilliseconds <= 0)
            {
                Console.WriteLine($"Expiry date already passed for companyId: {companyId}");
                return;
            }

            if (_schedules.ContainsKey(companyId))
            {
                _schedules[companyId].Stop();
                _schedules[companyId].Dispose();
            }

            var timer = new System.Timers.Timer(timeUntilExpiry.TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await OnTimerElapsed(companyId);
            timer.AutoReset = false;
            timer.Start();

            _schedules[companyId] = timer;

            Console.WriteLine($"Scheduled next scrape for companyId {companyId} at {expiryDate}");
        }

        private static async Task OnTimerElapsed(int companyId)
        {
            Console.WriteLine($"Running scheduled scrape for companyId: {companyId}");
            var latestAvis = await Operations.ScrapeAvis(companyId);

            if (latestAvis != null)
            {
                await _avisAPIRestClient.CreateAsync(latestAvis, companyId, TOKEN);
                Console.WriteLine($"Saved avis with externalid {latestAvis.ExternalId} to database");
                await ScheduleNextScrape(companyId, latestAvis.ValidTo);
            }
            Console.WriteLine($"Failed to scrape for companyId: {companyId}, rescheduling in 1 hour");
            latestAvis = await _avisAPIRestClient.GetValidAsync(companyId, TOKEN);

            await ScheduleNextScrape(companyId, latestAvis.ValidTo);
        }
    }
}
