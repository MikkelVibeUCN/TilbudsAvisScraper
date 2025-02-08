using APIIntegrationLibrary.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TilbudsAvisLibrary.DTO;

namespace AutomaticScraperConsoleApp
{
    public class Scheduler
    {
        private readonly ILogger<Scheduler> _logger;
        private static string token = "";
        private readonly IDatabaseService _databaseService;
        private readonly AvisAPIRestClient _avisAPIRestClient;
        private readonly ConcurrentDictionary<int, Timer> _timers = new();
        private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _retryBaseDelay = TimeSpan.FromMinutes(5);
        private const int MaxRetries = 3;

        public SmartScheduler(ILogger<Scheduler> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        public async Task InitializeAsync()
        {
            var companies = await _databaseService.GetActiveCompanies();
            foreach (var company in companies)
            {
                ScheduleNextScrape(company, immediate: false);
            }
        }

        private void ScheduleNextScrape(CompanyScheduleDTO company, bool immediate)
        {
            var dueTime = immediate ? _initialDelay : CalculateDueTime(company);

            _timers.AddOrUpdate(company.Id,
                id => new Timer(async _ => await ScrapeCompany(id), null, dueTime, Timeout.InfiniteTimeSpan),
                (id, existingTimer) =>
                {
                    existingTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
                    return existingTimer;
                });
        }

        private TimeSpan CalculateDueTime(CompanyScheduleDTO company)
        {
            var now = DateTime.UtcNow;
            return company.NextExpectedRelease > now
                ? company.NextExpectedRelease - now
                : _initialDelay;
        }

        private async Task ScrapeCompany(int companyId)
        {
            var company = await _databaseService.GetCompany(companyId);
            if (company == null || !company.IsActive) return;

            try
            {
                _logger.LogInformation("Starting scrape for {Company}", company.Name);

                var result = await Operations.ScrapeAvis(
                    company.Id,
                    progress => HandleProgress(company, progress),
                    new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token);

                if (result != null)
                {
                    await _avisAPIRestClient.CreateAsync(result, company.Id, token);
                    await _databaseService.UpdateCompanySchedule(company.Id, result.ValidTo, 0);
                    ScheduleNextScrape(company with { NextExpectedRelease = result.ValidTo }, immediate: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scrape failed for {CompanyId}", company.Id);
                var newRetryCount = Math.Min(company.RetryCount + 1, MaxRetries);
                var nextTry = DateTime.UtcNow + _retryBaseDelay * Math.Pow(2, newRetryCount);

                await _databaseService.UpdateCompanySchedule(
                    company.Id,
                    nextExpectedRelease: nextTry,
                    retryCount: newRetryCount);

                ScheduleNextScrape(company with
                {
                    NextExpectedRelease = nextTry,
                    RetryCount = newRetryCount
                }, immediate: false);
            }
        }

        private void HandleProgress(CompanyScheduleDTO company, int progress)
        {
            _logger.LogInformation("{Company}: Progress {Progress}%", company.Name, progress);
        }
    }
}