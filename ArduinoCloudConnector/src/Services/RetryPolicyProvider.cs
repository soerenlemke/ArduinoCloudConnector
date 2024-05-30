using System.Net;
using ArduinoCloudConnector.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace ArduinoCloudConnector.Services;

public class RetryPolicyProvider(
    ILogger<RetryPolicyProvider> logger,
    IOptions<ArduinoCloudClientOptions> options)
    : IRetryPolicyProvider
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), options.Value.RetryCount);
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                delay,
                (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        "Retry {retryAttempt} encountered an error: {outcome.Exception?.Message}. Waiting {timespan} before next retry.",
                        retryAttempt, outcome.Exception.Message, timespan);
                });

        return retryPolicy;
    }
}