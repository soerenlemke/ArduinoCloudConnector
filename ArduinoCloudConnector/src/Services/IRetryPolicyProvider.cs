using Polly;

namespace ArduinoCloudConnector.Services;

public interface IRetryPolicyProvider
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
}