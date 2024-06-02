using System.Net.Http.Headers;
using System.Text.Json;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ArduinoCloudConnector.Services
{
    public class TokenManagementService(
        HttpClient httpClient,
        ILogger<TokenManagementService> logger,
        IOptions<ArduinoCloudClientOptions> options,
        IRetryPolicyProvider retryPolicyProvider,
        IResponseHandler responseHandler)
        : ITokenManagementService
    {
        private const int TokenExpirationTimeInSeconds = 3600;
        private readonly ArduinoCloudClientOptions _options = options.Value;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = retryPolicyProvider.GetRetryPolicy();
        private string? _accessToken;
        private DateTime _accessTokenExpiration;
        private readonly object _lock = new();

        public async Task<string> GetAccessTokenAsync()
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _accessTokenExpiration)
                {
                    return _accessToken;
                }
            }

            var localTokenData = await LoadAccessTokenLocal();
            if (!string.IsNullOrEmpty(localTokenData?.AccessToken) && DateTime.UtcNow < localTokenData.TokenExpiration)
            {
                lock (_lock)
                {
                    _accessToken = localTokenData.AccessToken;
                    _accessTokenExpiration = localTokenData.TokenExpiration;
                }
                return localTokenData.AccessToken;
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _options.ClientId),
                    new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                    new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
                });
                tokenRequest.Content = content;
                tokenRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                return await httpClient.SendAsync(tokenRequest);
            });

            if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response);

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            lock (_lock)
            {
                _accessToken = tokenResponse?.AccessToken ?? string.Empty;
                _accessTokenExpiration = DateTime.UtcNow.AddSeconds(TokenExpirationTimeInSeconds);
            }

            SaveAccessTokenLocal(_accessToken, _accessTokenExpiration);

            logger.LogInformation("Access token received: {accessToken}", _accessToken);
            return _accessToken;
        }

        public async Task<TokenData?> LoadAccessTokenLocal()
        {
            try
            {
                var jsonFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AccessToken.json");
                if (!File.Exists(jsonFileName)) return null;

                var localTokenData = await File.ReadAllTextAsync(jsonFileName);
                return JsonSerializer.Deserialize<TokenData>(localTokenData);
            }
            catch (Exception ex)
            {
                logger.LogError("Error loading the access token locally: {errorMessage}", ex.Message);
                return null;
            }
        }

        public void SaveAccessTokenLocal(string accessToken, DateTime tokenExpiration)
        {
            try
            {
                var jsonFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AccessToken.json");

                var tokenData = new TokenData(accessToken, tokenExpiration);
                
                var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonStringToken = JsonSerializer.Serialize(tokenData, jsonSerializerOptions);
                File.WriteAllText(jsonFileName, jsonStringToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Error saving the access token locally: {errorMessage}", ex.Message);
            }
        }
    }
}
