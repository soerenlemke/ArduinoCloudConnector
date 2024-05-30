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
        var loggerFactory = LoggerFactory.Create(
            builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug)
        );
        var logger = loggerFactory.CreateLogger<Program>();

        
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                Env.Load();
                services.AddHttpClient<ArduinoCloudClient>();
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
        
        var thingId = Env.GetString("THING_ID");
        var arduinoCloudClient = host.Services.GetRequiredService<ArduinoCloudClient>();
        try
        {
            var thingProperties = await arduinoCloudClient.GetThingPropertiesAsync(thingId);
            if (thingProperties is null)
            {
                logger.LogError("No properties for the Thing: {thingId}", thingId);
                return;
            }

            foreach (var property in thingProperties)
                logger.LogInformation(
                    "Name: {property.Name}, Value: {property.LastValue}, Type: {property.Type}, Updated At: {property.ValueUpdatedAt}",
                    property.Name, property.LastValue, property.Type, property.ValueUpdatedAt);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get thing properties: {ex.Message}", ex.Message);
        }
    }
}