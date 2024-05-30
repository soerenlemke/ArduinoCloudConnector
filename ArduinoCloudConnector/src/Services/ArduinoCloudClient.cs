using System.Net;
using System.Net.Http.Headers;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ArduinoCloudConnector.Services;

public class ArduinoCloudClient(
    HttpClient httpClient,
    ILogger<ArduinoCloudClient> logger,
    IOptions<ArduinoCloudClientOptions> options)
{
    private async Task<string> GetAccessTokenAsync()
    {
        for (var i = 0; i < options.Value.RetryCount; i++)
            try
            {
                var tokenRequest =
                    new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", options.Value.ClientId),
                    new KeyValuePair<string, string>("client_secret", options.Value.ClientSecret),
                    new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
                });
                tokenRequest.Content = content;

                if (tokenRequest.Content != null)
                    tokenRequest.Content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(tokenRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    logger.LogError("Request failed with status code {response.StatusCode}: {errorResponse}",
                        response.StatusCode, errorResponse);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        logger.LogError(
                            "404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");
                    if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
                    {
                        logger.LogInformation("Server error. Retrying...");
                        await Task.Delay(options.Value.RetryDelay);
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
                logger.LogError("Error occurred: {ex.Message}", ex.Message);
                if (i == options.Value.RetryCount - 1) throw;
                await Task.Delay(options.Value.RetryDelay);
            }

        return string.Empty;
    }

    public async Task<List<ThingProperty>?> GetThingPropertiesAsync(string thingId)
    {
        for (var i = 0; i < options.Value.RetryCount; i++)
            try
            {
                logger.LogInformation("Getting access token for clientId: {options.Value.ClientId}",
                    options.Value.ClientId);
                var accessToken = await GetAccessTokenAsync();
                logger.LogInformation("Access token received: {accessToken}", accessToken);

                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                logger.LogInformation(
                    "Sending request to URL: https://api2.arduino.cc/iot/v2/things/{thingId}/properties", thingId);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    logger.LogError("Request failed with status code {response.StatusCode}: {errorResponse}",
                        response.StatusCode,
                        errorResponse);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        logger.LogError(
                            "Thing not found. Please check the thingId {thingId} and ensure the thing exists in your Arduino Cloud.",
                            thingId);

                    if (response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        logger.LogInformation("Server error. Retrying...");
                        await Task.Delay(options.Value.RetryDelay);
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
                logger.LogError("Error occurred: {ex.Message}", ex.Message);
                if (i == options.Value.RetryCount - 1) throw;
                await Task.Delay(options.Value.RetryDelay);
            }

        return null;
    }

    /*
    // Better Code
    TODO: Refactor Retry Logic: Move the retry logic to a separate method to avoid code duplication.
    TODO: Improve Error Handling: Use specific exceptions for different error types to make error handling more granular.
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