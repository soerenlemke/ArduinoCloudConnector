using System.Net;
using System.Net.Http.Headers;
using ArduinoCloudConnector.Exceptions;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;

namespace ArduinoCloudConnector.Services;

public class ArduinoCloudClient(
    HttpClient httpClient,
    ILogger<ArduinoCloudClient> logger,
    IOptions<ArduinoCloudClientOptions> options,
    IRetryPolicyProvider retryPolicyProvider)
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = retryPolicyProvider.GetRetryPolicy();

    private async Task<string> GetAccessTokenAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api2.arduino.cc/iot/v1/clients/token");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", options.Value.ClientId),
                new KeyValuePair<string, string>("client_secret", options.Value.ClientSecret),
                new KeyValuePair<string, string>("audience", "https://api2.arduino.cc/iot")
            });
            tokenRequest.Content = content;
            tokenRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            return await httpClient.SendAsync(tokenRequest);
        });

        if (!response.IsSuccessStatusCode)
        {
            await HandleUnsuccessfulResponseAsync(response);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

        return tokenResponse?.AccessToken ?? string.Empty;
    }

    public async Task<List<ThingProperty>?> GetThingPropertiesAsync(string thingId)
    {
        var accessToken = await GetAccessTokenAsync();
        logger.LogInformation("Access token received: {accessToken}", accessToken);

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.SendAsync(request);
        });

        if (!response.IsSuccessStatusCode)
        {
            await HandleUnsuccessfulResponseAsync(response, thingId);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<ThingProperty>>(responseBody);
    }

    private async Task HandleUnsuccessfulResponseAsync(HttpResponseMessage response, string? thingId = null)
    {
        var errorResponse = await response.Content.ReadAsStringAsync();
        logger.LogError("Request failed with status code {StatusCode}: {ErrorResponse}", response.StatusCode,
            errorResponse);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                if (thingId != null)
                {
                    logger.LogError("Thing not found. Please check the thingId {ThingId} and ensure the thing exists in your Arduino Cloud.", thingId);
                    throw new NotFoundException($"Thing not found: {thingId}");
                }
                else
                {
                    logger.LogError("404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");
                    throw new NotFoundException("404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");
                }

            case HttpStatusCode.InternalServerError:
                logger.LogInformation("Server error. Retrying...");
                throw new InternalServerErrorException("Internal Server Error. Please try again later.");

            case HttpStatusCode.ServiceUnavailable:
                logger.LogInformation("Service unavailable. Retrying...");
                throw new ServiceUnavailableException("Service Unavailable. Please try again later.");

            default:
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
        }
    }

    /*
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