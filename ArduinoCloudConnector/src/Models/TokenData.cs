namespace ArduinoCloudConnector.Models;

public class TokenData(string accessToken, DateTime tokenExpiration)
{
    public string AccessToken { get; set; } = accessToken;
    public DateTime TokenExpiration { get; set; } = tokenExpiration;
}