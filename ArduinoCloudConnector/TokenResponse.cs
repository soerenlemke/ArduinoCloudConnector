using Newtonsoft.Json;

namespace ArduinoCloudConnector;

public class TokenResponse(string accessToken)
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = accessToken;
}