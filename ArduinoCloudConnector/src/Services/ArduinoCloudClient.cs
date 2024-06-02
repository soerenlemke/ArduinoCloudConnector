using System.Net.Http.Headers;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;

namespace ArduinoCloudConnector.Services;

public class ArduinoCloudClient(
    HttpClient httpClient,
    ITokenManagementService tokenManagementService,
    IRetryPolicyProvider retryPolicyProvider,
    IResponseHandler responseHandler)
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = retryPolicyProvider.GetRetryPolicy();

    public async Task<List<Thing>?> GetThingsAsync()
    {
        var response = await SendRequestAsync("https://api2.arduino.cc/iot/v2/things");

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Thing>>(responseBody);
    }

    public async Task<List<ThingProperty>?> GetThingPropertiesAsync(string thingId)
    {
        var response = await SendRequestAsync($"https://api2.arduino.cc/iot/v2/things/{thingId}/properties");

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response, thingId);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<ThingProperty>>(responseBody);
    }

    public async Task<ThingProperty?> UpdateThingPropertyAsync(string thingId, string propertyId)
    {
        var response = await SendRequestAsync($"https://api2.arduino.cc/iot/v2/things/{thingId}/properties/{propertyId}");

        if (!response.IsSuccessStatusCode) await responseHandler.HandleUnsuccessfulResponseAsync(response);

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ThingProperty>(responseBody);
    }
    
    private async Task<HttpResponseMessage> SendRequestAsync(string url)
    {
        var accessToken = await tokenManagementService.GetAccessTokenAsync();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await httpClient.SendAsync(request);
        });
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