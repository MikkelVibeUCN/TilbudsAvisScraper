using System;
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
                await ScheduleInitialScrape(companyId);
            }
        }

        private static async Task ScheduleInitialScrape(int companyId)
        {
            var latestAvis = await _avisAPIRestClient.GetValidAsync(companyId, TOKEN);

            // For testing rescheduler
            //Random random = new Random();
            //bool createNull = random.Next(0, 2) == 1;
            //
            //if (createNull)
            //{
            //    latestAvis = null;
            //}

            if (latestAvis != null)
            {
                await ScheduleNextScrape(companyId, latestAvis.ValidTo.AddDays(1).AddHours(2));
            }
            else
            {
                Console.WriteLine($"Failed to fetch latest valid avis from database for companyId: {companyId}");
                Console.WriteLine("Rescheduling...");
                await ScheduleTaskAtSpecificTime(companyId, DateTime.Now.AddMinutes(1), async () => await ScheduleInitialScrape(companyId));
            }
        }

        private static async Task ScheduleTaskAtSpecificTime(int companyId, DateTime targetTime, Func<Task> taskToSchedule)
        {
            var timeUntilTarget = targetTime - DateTime.Now;

            if (timeUntilTarget.TotalMilliseconds <= 0)
            {
                await taskToSchedule();
                return;
            }

            if (_schedules.ContainsKey(companyId))
            {
                _schedules[companyId].Stop();
                _schedules[companyId].Dispose();
                _schedules.Remove(companyId);
            }

            var timer = new System.Timers.Timer(timeUntilTarget.TotalMilliseconds);
            timer.Elapsed += async (sender, e) =>
            {
                await taskToSchedule();
            };
            timer.AutoReset = false;
            timer.Start();

            _schedules[companyId] = timer;

            Console.WriteLine($"Scheduled task to run at {targetTime} with companyId: {companyId}");
        }


        private static async Task ScheduleNextScrape(int companyId, DateTime expiryDate)
        {
            var timeUntilExpiry = expiryDate - DateTime.Now;

            if (timeUntilExpiry.TotalMilliseconds <= 0)
            {
                Console.WriteLine($"Expiry date already passed for companyId: {companyId}");
                Console.WriteLine("Scraping now instead...");
                await SaveNewAvis(companyId);
                return;
            }

            await ScheduleTaskAtSpecificTime(companyId, expiryDate, async () => await SaveNewAvis(companyId));
        }


        private static async Task SaveNewAvis(int companyId)
        {
            Console.WriteLine($"Running scheduled scrape for companyId: {companyId}");
            var latestAvis = await Operations.ScrapeAvis(companyId);

            bool isSuccessful = false;

            if (latestAvis != null)
            {
                try
                {
                    if (latestAvis.Products.Count < 10)
                    {
                        throw new Exception("Not enough products, likely issue with scraper lib");
                    }

                    await _avisAPIRestClient.CreateAsync(latestAvis, companyId, TOKEN);
                    isSuccessful = true;
                    Console.WriteLine($"Saved avis with externalid {latestAvis.ExternalId} to database");
                    await ScheduleNextScrape(companyId, latestAvis.ValidTo.AddDays(1).AddHours(2));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to create avis in database for companyId: {companyId}");
                    Console.WriteLine(e.Message);
                }
            }

            if(!isSuccessful)
            {
                Console.WriteLine("Rescheduling...");

                await ScheduleNextScrape(companyId, DateTime.Now.AddHours(1));
            }
        }
    }
}