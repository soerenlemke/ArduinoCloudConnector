using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ArduinoCloudConnector;

public class ArduinoCloudClient(string clientId, string clientSecret)
{
    private readonly HttpClient _httpClient = new();

    private async Task<string> GetAccessTokenAsync()
    {
        const int retryCount = 3;
        const int delay = 2000;

        for (var i = 0; i < retryCount; i++)
            try
            {
                var tokenRequest =
                    new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
                });
                tokenRequest.Content = content;
                tokenRequest.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await _httpClient.SendAsync(tokenRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Request failed with status code {response.StatusCode}: {errorResponse}");

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        Console.WriteLine(
                            "404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");

                    // Retry on server errors or rate limiting
                    if (response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        Console.WriteLine("Server error. Retrying...");
                        Thread.Sleep(delay);
                        continue;
                    }

                    throw new HttpRequestException(
                        $"Request failed with status code {response.StatusCode}: {errorResponse}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

                return tokenResponse?.AccessToken ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                if (i == retryCount - 1) throw;
                Thread.Sleep(delay);
            }

        return string.Empty;
    }

    public async Task<List<ThingProperty>?> GetThingPropertiesAsync(string thingId)
    {
        const int retryCount = 3;
        const int delay = 2000;

        for (var i = 0; i < retryCount; i++)
            try
            {
                Console.WriteLine($"Getting access token for clientId: {clientId}");
                var accessToken = await GetAccessTokenAsync();
                Console.WriteLine($"Access token received: {accessToken}");

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                Console.WriteLine(
                    $"Sending request to URL: https://api2.arduino.cc/iot/v2/things/{thingId}/properties");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Request failed with status code {response.StatusCode}: {errorResponse}");

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        Console.WriteLine(
                            "Thing not found. Please check the thingId and ensure the thing exists in your Arduino Cloud.");

                    if (response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        Console.WriteLine("Server error. Retrying...");
                        Thread.Sleep(delay);
                        continue;
                    }

                    throw new HttpRequestException(
                        $"Request failed with status code {response.StatusCode}: {errorResponse}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var thingProperties = JsonConvert.DeserializeObject<List<ThingProperty>>(responseBody);
                return thingProperties;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                if (i == retryCount - 1) throw;
                Thread.Sleep(delay);
            }

        return null;
    }
}