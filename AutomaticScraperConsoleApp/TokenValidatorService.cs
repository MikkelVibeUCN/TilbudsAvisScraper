using APIIntegrationLibrary.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticScraperConsoleApp
{
    public class TokenValidatorService
    {
        private readonly APIUserRestClient _userClient;
        private readonly Action<string> _logAction;
        private readonly Action<string, Exception> _logErrorAction;

        // Configuration properties
        public int MaxRetries { get; set; } = 3;
        public int DelayMinutes { get; set; } = 1;
        public const int PermissionLevel = 3;

        public TokenValidatorService(APIUserRestClient userClient, Action<string> logAction, Action<string, Exception> logErrorAction = null)
        {
            _userClient = userClient ?? throw new ArgumentNullException(nameof(userClient));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _logErrorAction = logErrorAction ?? ((msg, ex) => _logAction($"ERROR: {msg} - {ex?.Message}"));
        }


        public async Task<bool> ValidateTokenAsync(int permissionLevel = PermissionLevel)
        {
            _logAction("Validating token...");

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _logAction($"Token validation attempt {attempt}/{MaxRetries}...");
                    bool isValid = await _userClient.IsTokenValidForAction(permissionLevel);
                    _logAction($"Token validation result: {isValid}");

                    if (isValid)
                    {
                        _logAction("Token validation successful.");
                        return true;
                    }
                    else
                    {
                        if (attempt < MaxRetries)
                        {
                            _logAction($"Token not valid. Retrying in {DelayMinutes} minute(s)...");
                            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes));
                        }
                        else
                        {
                            _logErrorAction("Token not valid after all retry attempts.", null);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < MaxRetries)
                    {
                        _logErrorAction($"Error during token validation (attempt {attempt}/{MaxRetries}). Retrying in {DelayMinutes} minute(s)...", ex);
                        await Task.Delay(TimeSpan.FromMinutes(DelayMinutes));
                    }
                    else
                    {
                        _logErrorAction($"Error during token validation after all retry attempts (attempt {attempt}/{MaxRetries})", ex);
                        return false;
                    }
                }
            }

            return false; // This should never be reached, but included for completeness
        }

        public async Task<bool> ValidateTokenOnceAsync(int permissionLevel = PermissionLevel)
        {
            try
            {
                _logAction("Validating token (single attempt)...");
                bool isValid = await _userClient.IsTokenValidForAction(permissionLevel);
                _logAction($"Token validation result: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logErrorAction("Error during token validation", ex);
                return false;
            }
        }
    }
}