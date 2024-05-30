using System.Net;
using System.Net.Http.Headers;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ArduinoCloudConnector.Services;

public class ArduinoCloudClient(HttpClient httpClient, IOptions<ArduinoCloudClientOptions> options)
{
    private async Task<string> GetAccessTokenAsync()
    {
        const int retryCount = 3;
        const int delay = 2000;

        for (var i = 0; i < retryCount; i++)
            try
            {
                var tokenRequest =
                    new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");

                if (options.Value is { ClientId: not null, ClientSecret: not null })
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", options.Value.ClientId),
                        new KeyValuePair<string, string>("client_secret", options.Value.ClientSecret),
                        new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
                    });
                    tokenRequest.Content = content;
                }

                if (tokenRequest.Content != null)
                    tokenRequest.Content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(tokenRequest);

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
                Console.WriteLine($"Getting access token for clientId: {options.Value.ClientId}");
                var accessToken = await GetAccessTokenAsync();
                Console.WriteLine($"Access token received: {accessToken}");

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                Console.WriteLine(
                    $"Sending request to URL: https://api2.arduino.cc/iot/v2/things/{thingId}/properties");

                var response = await httpClient.SendAsync(request);

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

    /*
    // Better Code
    TODO: Refactor Retry Logic: Move the retry logic to a separate method to avoid code duplication.
    TODO: Improve Error Handling: Use specific exceptions for different error types to make error handling more granular.
    TODO: Add Logging: Implement a logging mechanism instead of using Console.WriteLine.
    TODO: Asynchronous Programming: Use Task.Delay instead of Thread.Sleep for asynchronous delay.
    // Adding features
    TODO: CreateThingAsync: Create a new Thing in the Arduino IoT Cloud.
    TODO: UpdateThingPropertyAsync: Update a specific property of a Thing.
    TODO: DeleteThingAsync: Delete a Thing from the Arduino IoT Cloud.
    TODO: GetThingAsync: Fetch details of a specific Thing.
    TODO: ListThingsAsync: List all Things in the Arduino IoT Cloud.
    TODO: CreateThingPropertyAsync: Create a new property for a Thing.
    TODO: DeleteThingPropertyAsync: Delete a specific property from a Thing.
    */
}