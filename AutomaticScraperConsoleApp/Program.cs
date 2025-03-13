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
        private static Dictionary<DateTime, DateTime> minNextScrapeTimePerDay = new Dictionary<DateTime, DateTime>(); // Tracks next available time per day
        private static AvisAPIRestClient _avisAPIRestClient;

        static async Task Main(string[] args)
        {
            TOKEN = Environment.GetEnvironmentVariable("TOKEN");
            URI = Environment.GetEnvironmentVariable("API_URI");
            if (TOKEN == null || URI == null)
            {
                Console.WriteLine("TOKEN or API_URI environment variables are not set.");
                return;
            }

            APIUserRestClient _userRestClient = new APIUserRestClient(URI, TOKEN);
            if (!await _userRestClient.IsTokenValidForAction(3))
            {
                Console.WriteLine("Token not valid");
                return;
            }

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

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationTokenSource.Token);
            }

            Console.WriteLine("Application is shutting down...");
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
            try
            {
                var latestAvis = await _avisAPIRestClient.GetValidAsync(companyId, TOKEN);

                if (latestAvis != null)
                {
                    await ScheduleNextScrape(companyId, latestAvis.ValidTo.AddDays(1).AddHours(2));
                }
                else
                {
                    Console.WriteLine($"Failed to fetch latest valid avis, it was null for companyId: {companyId}");
                    Console.WriteLine("Rescheduling...");
                    await ScheduleTaskAtSpecificTime(companyId, DateTime.Now.AddMinutes(1), async () => await ScheduleInitialScrape(companyId));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception {e.Message} when fetching latest avis companyId: {companyId}");
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
            DateTime scrapeDate = expiryDate.Date; 
            DateTime scheduledTime;

            lock (minNextScrapeTimePerDay)
            {
                if (!minNextScrapeTimePerDay.ContainsKey(scrapeDate))
                {
                    scheduledTime = expiryDate;
                }
                else
                {
                    scheduledTime = minNextScrapeTimePerDay[scrapeDate].AddMinutes(15);
                }

                minNextScrapeTimePerDay[scrapeDate] = scheduledTime;
            }

            await ScheduleTaskAtSpecificTime(companyId, scheduledTime, async () => await SaveNewAvis(companyId));
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

            if (!isSuccessful)
            {
                Console.WriteLine("Rescheduling...");

                await ScheduleNextScrape(companyId, DateTime.Now.AddHours(1));
            }
        }
    }
}
