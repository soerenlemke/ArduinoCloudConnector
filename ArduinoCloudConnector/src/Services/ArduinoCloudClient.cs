using System.Net.Http.Headers;
using ArduinoCloudConnector.Models;
using Newtonsoft.Json;
using Polly;

namespace ArduinoCloudConnector.Services;

public class ArduinoCloudClient(
    HttpClient httpClient,
    IRetryPolicyProvider retryPolicyProvider,
    IResponseHandler responseHandler,
    ITokenManagementService tokenManagementService)
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = retryPolicyProvider.GetRetryPolicy();

    public async Task<List<Thing>?> GetThingsAsync()
    {
        var accessToken = await tokenManagementService.GetAccessTokenAsync();

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api2.arduino.cc/iot/v2/things");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.SendAsync(request);
        });

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Thing>>(responseBody);
    }

    public async Task<List<ThingProperty>?> GetThingPropertiesAsync(string thingId)
    {
        var accessToken = await tokenManagementService.GetAccessTokenAsync();

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.SendAsync(request);
        });

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response, thingId);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<ThingProperty>>(responseBody);
    }

    public async Task<ThingProperty?> UpdateThingPropertyAsync(string thingId, string propertyId)
    {
        var accessToken = await tokenManagementService.GetAccessTokenAsync();

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api2.arduino.cc/iot/v2/things/{thingId}/properties/{propertyId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.SendAsync(request);
        });

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ThingProperty>(responseBody);
    }

    /*
    // Adding features
    TODO: CreateThingAsync: Create a new Thing in the Arduino IoT Cloud.
    TODO: DeleteThingAsync: Delete a Thing from the Arduino IoT Cloud.
    TODO: ListThingsAsync: List all Things in the Arduino IoT Cloud.
    TODO: CreateThingPropertyAsync: Create a new property for a Thing.
    TODO: DeleteThingPropertyAsync: Delete a specific property from a Thing.
    */
}