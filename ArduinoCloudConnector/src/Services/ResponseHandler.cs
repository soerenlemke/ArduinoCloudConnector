using System.Net;
using ArduinoCloudConnector.Exceptions;
using Microsoft.Extensions.Logging;

namespace ArduinoCloudConnector.Services;

public class ResponseHandler(
    ILogger<TokenManagementService> logger)
    : IResponseHandler
{
    public async Task HandleUnsuccessfulResponseAsync(HttpResponseMessage response, string? thingId = null)
    {
        var errorResponse = await response.Content.ReadAsStringAsync();
        logger.LogError("Request failed with status code {StatusCode}: {ErrorResponse}", response.StatusCode,
            errorResponse);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                if (thingId != null)
                {
                    logger.LogError(
                        "Thing not found. Please check the thingId {ThingId} and ensure the thing exists in your Arduino Cloud.",
                        thingId);
                    throw new NotFoundException($"Thing not found: {thingId}");
                }

                logger.LogError("404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");
                throw new NotFoundException(
                    "404 Not Found error. URL or resource might be incorrect or temporarily unavailable.");

            case HttpStatusCode.InternalServerError:
                logger.LogError("Internal Server Error. Please try again later.");
                throw new InternalServerErrorException("Internal Server Error. Please try again later.");

            case HttpStatusCode.ServiceUnavailable:
                logger.LogError("Service Unavailable. Please try again later.");
                throw new ServiceUnavailableException("Service Unavailable. Please try again later.");

            case HttpStatusCode.BadRequest:
                logger.LogError("Bad Request. The request could not be understood or was missing required parameters.");
                throw new HttpRequestException($"Bad Request: {errorResponse}");

            case HttpStatusCode.Unauthorized:
                logger.LogError(
                    "Unauthorized. Authentication is required and has failed or has not yet been provided.");
                throw new HttpRequestException($"Unauthorized: {errorResponse}");

            case HttpStatusCode.Forbidden:
                logger.LogError("Forbidden. The server understood the request, but is refusing to fulfill it.");
                throw new HttpRequestException($"Forbidden: {errorResponse}");

            case HttpStatusCode.RequestTimeout:
                logger.LogError("Request Timeout. The server timed out waiting for the request.");
                throw new HttpRequestException($"Request Timeout: {errorResponse}");

            case HttpStatusCode.Conflict:
                logger.LogError(
                    "Conflict. The request could not be completed due to a conflict with the current state of the resource.");
                throw new HttpRequestException($"Conflict: {errorResponse}");

            case HttpStatusCode.Gone:
                logger.LogError("Gone. The requested resource is no longer available and will not be available again.");
                throw new HttpRequestException($"Gone: {errorResponse}");

            case HttpStatusCode.UnsupportedMediaType:
                logger.LogError(
                    "Unsupported Media Type. The request entity has a media type which the server or resource does not support.");
                throw new HttpRequestException($"Unsupported Media Type: {errorResponse}");

            case HttpStatusCode.TooManyRequests:
                logger.LogError("Too Many Requests. The user has sent too many requests in a given amount of time.");
                throw new HttpRequestException($"Too Many Requests: {errorResponse}");

            default:
                logger.LogError("Request failed with status code {StatusCode}: {ErrorResponse}", response.StatusCode,
                    errorResponse);
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}: {errorResponse}");
        }
    }
}