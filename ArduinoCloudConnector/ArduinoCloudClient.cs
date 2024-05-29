using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

namespace ArduinoCloudConnector
{
    public class ArduinoCloudClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public ArduinoCloudClient(string clientId, string clientSecret)
        {
            _httpClient = new HttpClient();
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            int retryCount = 3;
            int delay = 2000; // 2 seconds

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("client_secret", _clientSecret),
                        new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
                    });
                    tokenRequest.Content = content;
                    tokenRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await _httpClient.SendAsync(tokenRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request failed with status code {response.StatusCode}: {errorResponse}");
                        
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // Log details for debugging
                            Console.WriteLine("404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");
                        }

                        // Retry on server errors or rate limiting
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            Console.WriteLine("Server error. Retrying...");
                            Thread.Sleep(delay);
                            continue;
                        }
                        
                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

                    return tokenResponse?.AccessToken ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    if (i == retryCount - 1) // If the last retry also fails
                    {
                        throw;
                    }
                    Thread.Sleep(delay);
                }
            }
            return string.Empty;
        }

        public async Task<string> GetThingPropertiesAsync(string thingId)
        {
            int retryCount = 3;
            int delay = 2000; // 2 seconds

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Console.WriteLine($"Getting access token for clientId: {_clientId}");
                    var accessToken = await GetAccessTokenAsync();
                    Console.WriteLine($"Access token received: {accessToken}");

                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");

                    Console.WriteLine($"Sending request to URL: https://api2.arduino.cc/iot/v2/things/{thingId}/properties");

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request failed with status code {response.StatusCode}: {errorResponse}");

                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Console.WriteLine("Thing not found. Please check the thingId and ensure the thing exists in your Arduino Cloud.");
                        }

                        // Retry on server errors or rate limiting
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            Console.WriteLine("Server error. Retrying...");
                            Thread.Sleep(delay);
                            continue;
                        }

                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    if (i == retryCount - 1) // If the last retry also fails
                    {
                        throw;
                    }
                    Thread.Sleep(delay);
                }
            }
            return string.Empty;
        }
    }
}