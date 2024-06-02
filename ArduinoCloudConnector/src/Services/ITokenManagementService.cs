namespace ArduinoCloudConnector.Services;

public interface ITokenManagementService
{
    Task<string> GetAccessTokenAsync();
}