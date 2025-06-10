using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using APIIntegrationLibrary.Client;
using TilbudsAvisLibrary.DTO;
using ScraperLibrary.Exceptions;

namespace AutomaticScraperConsoleApp
{
    class Program
    {
        private static string URI;
        private static string TOKEN;
        private static AvisAPIRestClient _avisAPIRestClient;

        private static readonly Dictionary<int, CancellationTokenSource> _scraperCts = new();
        private static readonly object _scheduleLock = new object();
        private static readonly Dictionary<DateTime, DateTime> _minNextScrapeTimePerDay = new();

        // Custom logging method with timestamp
        private static readonly string _logFilePath = "scraper_log.txt";
        private static void Log(string message)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            Console.WriteLine(logEntry);
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }

        private static void LogError(string message, Exception ex = null)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ERROR: {message}";
            Console.WriteLine(logEntry);
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);

            if (ex != null)
            {
                var exceptionLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Exception: {ex.Message}\n{ex.StackTrace}";
                Console.WriteLine(exceptionLog);
                File.AppendAllText(_logFilePath, exceptionLog + Environment.NewLine);
            }
        }


        static async Task Main(string[] args)
        {
            try
            {
                Log("Application starting...");

                TOKEN = Environment.GetEnvironmentVariable("TOKEN");
                URI = Environment.GetEnvironmentVariable("API_URI");

                Log($"API URI: {URI ?? "Not set"}");
                Log($"Token available: {!string.IsNullOrEmpty(TOKEN)}");

                if (string.IsNullOrEmpty(TOKEN) || string.IsNullOrEmpty(URI))
                {
                    LogError("TOKEN or API_URI environment variables are not set.");
                    return;
                }

                Log("Validating token...");
                var userClient = new APIUserRestClient(URI, TOKEN);

                // Initialize the token validator service
                var tokenValidator = new TokenValidatorService(
                    userClient,
                    Log,
                    LogError
                );

                // Validate the token
                bool isTokenValid = await tokenValidator.ValidateTokenAsync();

                if (!isTokenValid)
                {
                    return; // Exit if token validation failed
                }

                // Continue with your application logic...

                _avisAPIRestClient = new AvisAPIRestClient(URI, TOKEN);
                Log("API client initialized");
                Log("Starting scraper...");

                try
                {
                    Log("Attempting to schedule all scrapers...");
                    await ScheduleAllScrapers();
                }
                catch (Exception ex)
                {
                    LogError("Error in ScheduleAllScrapers", ex);
                }

                // Memory log every minute
                Log("Starting memory monitoring...");
                using var logCts = new CancellationTokenSource();
                var memoryLogTask = Task.Run(async () =>
                {
                    while (!logCts.Token.IsCancellationRequested)
                    {
                        Log($"Memory usage: {GC.GetTotalMemory(true) / (1024 * 1024)} MB");
                        await Task.Delay(TimeSpan.FromMinutes(1), logCts.Token);
                    }
                }, logCts.Token);

                // Keep running
                Log("Entering main wait loop...");
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => {
                    Log("Cancel key pressed, shutting down...");
                    e.Cancel = true;
                    logCts.Cancel();
                    cts.Cancel();

                    // Cancel all scraper tasks
                    foreach (var scraperCts in _scraperCts.Values)
                    {
                        try { scraperCts.Cancel(); } catch { }
                    }
                };

                try
                {
                    await Task.Delay(Timeout.Infinite, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    Log("Application shutting down...");
                }
            }
            catch (Exception ex)
            {
                LogError("CRITICAL ERROR in Main", ex);
            }
        }



        private static async Task ScheduleAllScrapers()
        {
            try
            {
                Log("Getting scrapers from Operations.GetScrapers()...");
                var scrapers = Operations.GetScrapers();
                Log($"Retrieved {scrapers.Count} scrapers");

                if (scrapers.Count == 0)
                {
                    Log("WARNING: No scrapers found to schedule!");
                    return;
                }

                var companyIds = scrapers.Keys;

                foreach (var id in companyIds)
                {
                    Log($"Scheduling initial scrape for company ID: {id}");
                    try
                    {
                        await ScheduleInitialScrape(id);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error scheduling company {id}", ex);
                    }

                    // Small delay to avoid overwhelming the system with simultaneous API calls
                    await Task.Delay(100);
                }

                Log($"Finished scheduling all {companyIds.Count} companies");
            }
            catch (Exception ex)
            {
                LogError("Error in ScheduleAllScrapers", ex);
                throw; // Re-throw to let the caller handle
            }
        }

        private static async Task ScheduleInitialScrape(int companyId)
        {
            try
            {
                Log($"Getting latest avis for company {companyId}...");
                var latestAvis = await _avisAPIRestClient.GetValidAsync(companyId, TOKEN);

                DateTime targetTime;
                if (latestAvis != null)
                {
                    Log($"Found latest avis for company {companyId}, valid until {latestAvis.ValidTo}");
                    targetTime = latestAvis.ValidTo.AddDays(1).AddHours(2);
                }
                else
                {
                    Log($"No valid avis found for company {companyId}, scheduling immediate scrape");
                    targetTime = DateTime.Now.AddMinutes(1);
                }

                Log($"Scheduling company {companyId} for initial scrape at {targetTime}");

                // Define the scraping action
                await ScheduleTaskAt(companyId, targetTime, async (id) => {
                    Log($"Executing initial scheduled scrape for company {id}");
                    await SaveNewAvis(id);
                });
            }
            catch (Exception ex)
            {
                LogError($"Error in ScheduleInitialScrape for company {companyId}", ex);

                // Reschedule soon with backoff
                Log($"Scheduling retry for company {companyId} after error");
                await ScheduleTaskAt(companyId, DateTime.Now.AddMinutes(1), async (id) => {
                    await SaveNewAvis(id);
                });
            }
        }

        private static async Task ScheduleNextScrape(int companyId, DateTime validTo)
        {
            try
            {
                DateTime scrapeDay = validTo.Date;
                DateTime scheduleTime;

                lock (_scheduleLock)
                {
                    CleanupOldScrapeTimes();

                    if (!_minNextScrapeTimePerDay.TryGetValue(scrapeDay, out scheduleTime))
                        scheduleTime = validTo;
                    else
                        scheduleTime = scheduleTime.AddMinutes(15);

                    _minNextScrapeTimePerDay[scrapeDay] = scheduleTime;
                }

                Log($"Scheduling next scrape for company {companyId} at {scheduleTime}");
                await ScheduleTaskAt(companyId, scheduleTime, SaveNewAvis);
            }
            catch (Exception ex)
            {
                LogError($"Error in ScheduleNextScrape for company {companyId}", ex);

                // Fallback scheduling
                Log($"Fallback scheduling for company {companyId} in 30 minutes");
                await ScheduleTaskAt(companyId, DateTime.Now.AddMinutes(30), SaveNewAvis);
            }
        }

        private static void CleanupOldScrapeTimes()
        {
            try
            {
                var expired = new List<DateTime>();
                foreach (var key in _minNextScrapeTimePerDay.Keys)
                {
                    if (key < DateTime.Today.AddDays(-1))
                        expired.Add(key);
                }

                foreach (var key in expired)
                    _minNextScrapeTimePerDay.Remove(key);

                if (expired.Count > 0)
                {
                    Log($"Cleaned up {expired.Count} expired scrape times");
                }
            }
            catch (Exception ex)
            {
                LogError("Error in CleanupOldScrapeTimes", ex);
            }
        }

        private static async Task ScheduleTaskAt(int companyId, DateTime time, Func<int, Task> action)
        {
            try
            {
                // Cancel any existing task for this company
                if (_scraperCts.TryGetValue(companyId, out var existingCts))
                {
                    Log($"Cancelling existing task for company {companyId}");
                    existingCts.Cancel();
                    existingCts.Dispose();
                    _scraperCts.Remove(companyId);
                }

                var delay = time - DateTime.Now;
                if (delay.TotalMilliseconds <= 0)
                {
                    Log($"Executing immediate action for company {companyId} (no delay)");
                    await action(companyId);
                    return;
                }

                // Create a new cancellation token source for this task
                var cts = new CancellationTokenSource();
                _scraperCts[companyId] = cts;

                Log($"Scheduled company {companyId} to run in {delay.TotalMinutes:F1} minutes at {time:HH:mm:ss}");

                // Run the delayed task in a separate method to avoid closure issues
                _ = RunDelayedTask(companyId, delay, cts, action);
            }
            catch (Exception ex)
            {
                LogError($"Error in ScheduleTaskAt for company {companyId}", ex);
            }
        }

        private static async Task RunDelayedTask(int companyId, TimeSpan delay, CancellationTokenSource cts, Func<int, Task> action)
        {
            try
            {
                Log($"Starting delay of {delay.TotalMinutes:F1} minutes for company {companyId}");
                await Task.Delay(delay, cts.Token);

                // Only execute if not canceled
                if (!cts.Token.IsCancellationRequested)
                {
                    Log($"Delay completed, executing action for company {companyId}");
                    await action(companyId);
                }
            }
            catch (TaskCanceledException)
            {
                Log($"Task for company {companyId} was canceled");
            }
            catch (Exception ex)
            {
                LogError($"Error in delayed task for company {companyId}", ex);

                // Reschedule on error after a short delay
                if (!cts.IsCancellationRequested)
                {
                    Log($"Rescheduling company {companyId} after error");
                    await ScheduleTaskAt(companyId, DateTime.Now.AddMinutes(5), action);
                }
            }
            finally
            {
                // Clean up resources
                if (_scraperCts.TryGetValue(companyId, out var currentCts) && currentCts == cts)
                {
                    _scraperCts.Remove(companyId);
                    cts.Dispose();
                }
            }
        }

        private static async Task SaveNewAvis(int companyId)
        {
            try
            {
                Log($"Starting scrape for company {companyId}");
                var avis = await Operations.ScrapeAvis(companyId);

                Log($"Scraped {avis.Products.Count} products for company {companyId}");

                if (avis.Products.Count < 10)
                {
                    Log($"WARNING: Only {avis.Products.Count} products scraped for company {companyId}");
                    throw new Exception("Too few products scraped.");
                }

                Log($"Saving avis for company {companyId}");
                await _avisAPIRestClient.CreateAsync(avis, companyId, TOKEN);
                Log($"Successfully saved avis {avis.ExternalId} for company {companyId}");

                Log($"Scheduling next scrape for company {companyId}");
                await ScheduleNextScrape(companyId, avis.ValidTo.AddDays(1).AddHours(2));
            }
            catch (FutureValidFromException ex)
            {
                Log($"Avis for company {companyId} not valid yet, valid from: {ex.ValidFrom}");
                await ScheduleNextScrape(companyId, ex.ValidFrom.AddMinutes(5));
            }
            catch (Exception ex)
            {
                LogError($"Error saving avis for company {companyId}", ex);
                await ScheduleNextScrape(companyId, DateTime.Now.AddHours(1));
            }
        }
    }
}