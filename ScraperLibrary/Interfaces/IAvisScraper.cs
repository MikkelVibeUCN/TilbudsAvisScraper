using System.Globalization;
using System.Text.RegularExpressions;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary.Interfaces
{
    public interface IAvisScraper
    {
        Task<AvisDTO> GetAvis(Action<int> progressCallback, CancellationToken token, int companyId);

        public static (DateTime ValidFrom, DateTime ValidTo) ExtractAvisPeriodFromHTML(string html)
        {
            // Match the pattern "Avisen gælder fra <weekday> <dd.MM.yyyy> til og med <weekday> <dd.MM.yyyy>"
            var match = Regex.Match(
                html,
                @"Avisen gælder fra \w+ (\d{2}\.\d{2}\.\d{4}) til og med \w+ (\d{2}\.\d{2}\.\d{4})",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                if (DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                    DateTime.TryParseExact(match.Groups[2].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return (startDate, endDate);
                }
            }
            throw new Exception("Failed to extract valid dates from HTML. Please check the HTML structure or the regex pattern.");
        }
    }
}
