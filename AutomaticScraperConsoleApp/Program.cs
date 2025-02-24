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
        private const string URI = "http://api.tilbudsfinder.dk/v1/";
        private static readonly Dictionary<int, System.Timers.Timer> _schedules = new Dictionary<int, System.Timers.Timer>();
        private static readonly AvisAPIRestClient _avisAPIRestClient = new AvisAPIRestClient(URI);
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Automatic Scraper Console App...");
            await ScheduleAllScrapers();
            Console.ReadLine();
        }

        private static async Task ScheduleAllScrapers()
        {
            var companyIds = Operations.GetScrapers().Keys;

            foreach (var companyId in companyIds)
            {
                var latestAvis = await Operations.ScrapeAvis(companyId);

                if (latestAvis != null)
                {
                    ScheduleNextScrape(companyId, latestAvis.ValidTo);
                }
            }
        }

        private static void ScheduleNextScrape(int companyId, DateTime expiryDate)
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
                ScheduleNextScrape(companyId, latestAvis.ValidTo);
            }
            else
            {
                AvisDTO previousAvis = await _avisAPIRestClient.Get(companyId);
                Console.WriteLine($"The avis couldnt be found for companyId: {companyId}. Scheduling the same avis in 1 hour");
                ScheduleNextScrape(companyId, )
            }
        }
    }
}
