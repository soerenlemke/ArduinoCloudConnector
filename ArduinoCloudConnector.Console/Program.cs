using ArduinoCloudConnector.Models;
using ArduinoCloudConnector.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArduinoCloudConnector.Console;

internal class Program
{
    public static async Task Main()
    {
        Env.Load();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient<ArduinoCloudClient>()
                    .AddPolicyHandler((provider, _) =>
                    {
                        var retryPolicyProvider = provider.GetRequiredService<IRetryPolicyProvider>();
                        return retryPolicyProvider.GetRetryPolicy();
                    });
                services.AddTransient<IRetryPolicyProvider, RetryPolicyProvider>();
                services.AddLogging(config =>
                {
                    config.AddConsole();
                    config.AddDebug();
                    config.SetMinimumLevel(LogLevel.Debug);
                });
                services.Configure<ArduinoCloudClientOptions>(options =>
                {
                    options.ClientId = Env.GetString("CLIENT_ID");
                    options.ClientSecret = Env.GetString("CLIENT_SECRET");
                    options.RetryCount = 3;
                    options.RetryDelay = 2000;
                });
            })
            .Build();

        using var serviceScope = host.Services.CreateScope();
        var services = serviceScope.ServiceProvider;

        var arduinoCloudClient = services.GetRequiredService<ArduinoCloudClient>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Get all properties of a given Thing
        try
        {
            var thingId = Env.GetString("THING_ID");
            var thingProperties = await arduinoCloudClient.GetThingPropertiesAsync(thingId);
            if (thingProperties is null)
            {
                logger.LogError("No properties for the Thing: {thingId}", thingId);
                return;
            }

            foreach (var property in thingProperties)
            {
                logger.LogInformation(
                    "ID: {Id}, Name: {Name}, Value: {Value}, Type: {Type}, Updated At: {ValueUpdatedAt}",
                    property.Id, property.Name, property.LastValue, property.Type, property.ValueUpdatedAt);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get thing properties: {Message}", ex.Message);
        }
        
        // request a single property
        try
        {
            var thingId = Env.GetString("THING_ID");
            var propertyId = Env.GetString("PROPERTY_ID_1");
            var thingProperty = await arduinoCloudClient.UpdateThingPropertyAsync(thingId, propertyId);
            if (thingProperty != null)
            {
                logger.LogInformation("{Name} = {LastValue}", thingProperty.Name ,thingProperty.LastValue);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to request the property: {Message}", ex.Message);
        }
        
    }
}