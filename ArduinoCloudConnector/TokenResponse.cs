using Newtonsoft.Json;

namespace ArduinoCloudConnector;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string? AccessToken { get; set; }
}