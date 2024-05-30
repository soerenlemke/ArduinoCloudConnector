namespace ArduinoCloudConnector.Exceptions
{
    public class NotFoundException(string message) : HttpRequestException(message);
    public class InternalServerErrorException(string message) : HttpRequestException(message);
    public class ServiceUnavailableException(string message) : HttpRequestException(message);
}