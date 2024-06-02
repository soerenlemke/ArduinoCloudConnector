namespace ArduinoCloudConnector.Services;

public interface IResponseHandler
{
    Task HandleUnsuccessfulResponseAsync(HttpResponseMessage response, string? thingId = null);
}