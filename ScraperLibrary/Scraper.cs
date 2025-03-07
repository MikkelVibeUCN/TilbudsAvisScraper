﻿using PuppeteerSharp;
using System.Diagnostics;

namespace ScraperLibrary
{
    public abstract class Scraper
    {
        protected HttpClient client = new HttpClient();
        public static async Task<string> CallUrl(string fullUrl, int additionalDelayMs = 0)
        {
            // Download the browser if necessary
            await new BrowserFetcher().DownloadAsync();

            // Randomize user agents
            var random = new Random();
            {
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--disable-web-security", "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
                });

                using var page = await browser.NewPageAsync();

                // Set extra HTTP headers, including randomized user-agent
                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
                        {
                        { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36" },
                        { "upgrade-insecure-requests", "1" },
                        { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8" },
                        { "accept-encoding", "gzip, deflate, br" },
                        { "accept-language", "en-US,en;q=0.9,en;q=0.8" }
                });

                await page.GoToAsync(fullUrl);

                await page.WaitForSelectorAsync("main");

                await Task.Delay(500 + random.Next(500) + additionalDelayMs);

                var content = await page.GetContentAsync();

                return content;
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
    }
}