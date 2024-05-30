namespace ArduinoCloudConnector.Models;

public class ArduinoCloudClientOptions
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required int RetryCount { get; set; }
    public required int RetryDelay { get; set; }
}