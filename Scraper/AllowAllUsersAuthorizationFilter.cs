namespace Scraper
{
    using Hangfire.Dashboard;

    public class AllowAllUsersAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}