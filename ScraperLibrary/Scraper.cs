using PuppeteerSharp;
using System.Diagnostics;
using System.Threading;
using TilbudsAvisLibrary.DTO;

namespace ScraperLibrary
{
    public abstract class Scraper
    {
        protected HttpClient client = new HttpClient();
        private static IBrowser _browser;

        protected static async Task<IBrowser> GetBrowser()
        {
            if (_browser == null)
            {
                await new BrowserFetcher().DownloadAsync();

                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Timeout = 30000,
                    Args = new[] { "--disable-web-security", "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled", "--disable-features=AudioServiceOutOfProcess", "--disable-features=UseOzonePlatform" }
                });
            }
            return _browser;
        }

        public static async Task<string> CallUrl(string fullUrl, int additionalDelayMs = 0)
        {
            IBrowser browser = await GetBrowser();

            var random = new Random();
            {
                using var page = await browser.NewPageAsync();

                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                await page.GoToAsync(fullUrl, new NavigationOptions
                {
                    Timeout = 30000,
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } 
                });

                await page.WaitForSelectorAsync("main");

                await Task.Delay(1500 + random.Next(5000) + additionalDelayMs);

                var getContentTask = page.GetContentAsync();
                var timeoutTask = Task.Delay(30000);

                if (await Task.WhenAny(getContentTask, timeoutTask) == getContentTask)
                {
                    return await getContentTask;
                }

                throw new TimeoutException($"GetContentAsync timed out");
            }
        }

        public static T ExtractValueFromHtml<T>(string htmlContent, string startPattern, string endPattern, bool caseSensitive, int startOffset)
        {
            if (typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(string))
                throw new InvalidOperationException("The specified type is not supported. Only int, float, and string are allowed.");

            if (string.IsNullOrEmpty(htmlContent))
                throw new ArgumentException("The HTML content cannot be null or empty.");

            if (startOffset > htmlContent.Length)
                throw new ArgumentException("The start offset is greater than the length of the HTML content.");

            if (startPattern == endPattern)
                throw new ArgumentException("The start and end patterns cannot be the same.");

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int startIndex = htmlContent.IndexOf(startPattern, startOffset, comparison);
            int endIndex = htmlContent.IndexOf(endPattern, startOffset, comparison);

            if (startIndex > endIndex)
            {
                Console.WriteLine("Swapping start and end patterns.");
                (startPattern, endPattern) = (endPattern, startPattern);
            }

            if (startIndex == -1 || endIndex == -1)
                throw new InvalidOperationException("Start or end pattern not found in the HTML content.");

            startIndex += startPattern.Length;
            string extractedString = htmlContent.Substring(startIndex, endIndex - startIndex);

            try
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(extractedString, CultureInfo.InvariantCulture);
                else if (typeof(T) == typeof(float))
                    return (T)(object)float.Parse(extractedString, CultureInfo.InvariantCulture);
                else
                    return (T)(object)extractedString;
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"Failed to convert extracted string to the specified type. {typeof(T).Name}", ex);
            }
        }



        protected static dynamic GetInformationFromHtml<T>(string html, string searchPattern, string startSearchKey, string endSearchKey, int startIndexModifier = 0, bool ignoreCase = false)
        {
            if(ignoreCase)
            {
                html = html.ToLower();
                searchPattern = searchPattern.ToLower();
                startSearchKey = startSearchKey.ToLower();
                endSearchKey = endSearchKey.ToLower();
            }

            int startIndex = html.IndexOf(searchPattern);
            if (startIndex != -1 && html.Contains(startSearchKey))
            {
                startIndex = html.IndexOf(startSearchKey, startIndex + startIndexModifier) + startSearchKey.Length; // Move past the startSearchKey
                int endIndex = html.IndexOf(endSearchKey, startIndex); // Find the closing tag
                string information = html.Substring(startIndex, endIndex - startIndex).Trim();

                if (typeof(T) == typeof(float)) { return float.Parse(information); }
                else if (typeof(T) == typeof(int)) { return int.Parse(information); }
                else if (typeof(T) == typeof(string)) { return information; }
                else throw new InvalidCastException($"{typeof(T).FullName} is not supported");
            }
            throw new KeyNotFoundException($"searchpattern: {searchPattern} wasn't found");
        }
        protected static DateTime ConvertStringToDate(string date, IFormatProvider originalDateFormat, IFormatProvider newDateFormat)
        {
            DateTime dateTime = DateTime.Parse(date, originalDateFormat);

            DateTime convertedDateTime = DateTime.Parse(dateTime.ToString(newDateFormat), newDateFormat);

            return convertedDateTime;
        }

        // Changes any estimates in the string to their average. If the estimate check was faulty (fx sometimes they use "-" in kg-pris) it will just return the original string
        private static string ChangeDescriptionAccordingToOperation(string description, int locationToTakeAverage, char searchChar, Func<float, float, float> operation)
        {
            // Remove empty strings before the numbers if they exist
            if (description[locationToTakeAverage + 1].Equals(' ') && description[locationToTakeAverage - 1].Equals(' '))
            {
                description = description.Remove(locationToTakeAverage - 1, 1);
                description = description.Remove(locationToTakeAverage, 1);

                locationToTakeAverage = description.IndexOf(searchChar, StringComparison.OrdinalIgnoreCase);
                searchChar = description[locationToTakeAverage];
            }

            float firstNumber = GetValidNumber(locationToTakeAverage - 1, description, true);
            float secondNumber = GetValidNumber(locationToTakeAverage + 1, description, false);

            if (firstNumber != 0 && secondNumber != 0)
            {
                float result = operation(firstNumber, secondNumber);

                description = description.Replace($"{firstNumber}{searchChar}{secondNumber.ToString().Replace('.', ',')}", result.ToString());
            }
            return description;
        }

        protected static string ChangeMultiplyToOneValue(string description, int location, char searchChar)
        {
            return ChangeDescriptionAccordingToOperation(description, location, searchChar, (x, y) => x * y);
        }

        protected static string ChangeEstimateToAverage(string description, int location, char searchChar)
        {
            return ChangeDescriptionAccordingToOperation(description, location, searchChar, (x, y) => (x + y) / 2);
        }

        private static float GetNumber(int startIndex, string description, bool isFirstNumber)
        {
            int length = 0;
            int currentIndex = startIndex;

            while (currentIndex >= 0 && currentIndex < description.Length && CheckCharIsValidNumber(description[currentIndex]))
            {
                length++;
                currentIndex = isFirstNumber ? currentIndex - 1 : currentIndex + 1;
            }

            int start = isFirstNumber ? currentIndex + 1 : startIndex;
            string numberString = description.Substring(start, length);

            numberString = numberString.Replace(',', '.');

            return float.Parse(numberString);
        }

        private static float GetValidNumber(int index, string description, bool isFirstNumber)
        {
            char currentChar = description[index];
            if (CheckCharIsValidNumber(currentChar))
            {
                return GetNumber(index, description, isFirstNumber);
            }
            return 0;
        }

        private static bool CheckCharIsValidNumber(char charToCheck)
        {
            return (float.TryParse(charToCheck.ToString(), out _) && charToCheck != ' ') || charToCheck.Equals(',');
        }

        // Standard is kg, g, ml, cl, ltr, stk, bakke
        protected static string[] ConvertUnitsToStandard(string[] unitsToConvert)
        {
            List<string> convertedUnits = new();

            foreach (var unit in unitsToConvert)
            {
                switch (unit)
                {
                    case "GR.":
                        convertedUnits.Add("g");
                        break;
                    case "KG.":
                        convertedUnits.Add("kg");
                        break;
                    case "LTR.":
                        convertedUnits.Add("ltr");
                        break;
                    case "ML.":
                        convertedUnits.Add("ml");
                        break;
                    case "CL.":
                        convertedUnits.Add("cl");
                        break;
                    case "BAKKE":
                        convertedUnits.Add("bakke");
                        break;
                    case "STK.":
                    case "POSE":
                    case "PK.":
                    case "PAKKE":
                        convertedUnits.Add("stk");
                        break;
                    default:
                        throw new Exception("Unit not recognized");
                }
            }
            return convertedUnits.ToArray();
        }
        public static AvisDTO RemoveDuplicateProductsFromAvis(AvisDTO avis)
        {
            HashSet<string> externalIds = new HashSet<string>();

            foreach (ProductDTO product in avis.Products.ToList())
            {
                if (externalIds.Contains(product.ExternalId))
                {
                    avis.Products.Remove(product);
                }
                else
                {
                    externalIds.Add(product.ExternalId);
                }
            }

            return avis;
        }
    }
}