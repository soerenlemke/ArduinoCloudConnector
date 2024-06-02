using ArduinoCloudConnector.Models;
using ArduinoCloudConnector.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                services.AddTransient<IResponseHandler, ResponseHandler>();
                services.AddTransient<ITokenManagementService, TokenManagementService>();
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

        // Get all things of user
        try
        {
            var things = await arduinoCloudClient.GetThingsAsync();
            if (things is null)
            {
                var options = services.GetRequiredService<IOptions<ArduinoCloudClientOptions>>();
                logger.LogError("No things for the user with client Id: {clientId}", options.Value.ClientId);
                return;
            }

            foreach (var thing in things)
                logger.LogInformation(
                    "DeviceId: {DeviceId}, Id: {Id}, Name: {Name}, Properties: {Properties}, Tags: {Tags}, Timezone: {Timezone}, WebhookActive: {WebhookActive}, WebhookUri: {WebhookUri}",
                    thing.DeviceId, thing.Id, thing.Name, thing.Properties, thing.Tags, thing.Timezone,
                    thing.WebhookActive, thing.WebhookUri);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get things: {Message}", ex.Message);
        }
        
        // Get all devices of user
        try
        {
            var devices = await arduinoCloudClient.GetDevicesAsync();
            if (devices is null)
            {
                var options = services.GetRequiredService<IOptions<ArduinoCloudClientOptions>>();
                logger.LogError("No devices for the user with client Id: {clientId}", options.Value.ClientId);
                return;
            }

            foreach (var device in devices)
                logger.LogInformation(
                    "ConnectionType: {ConnectionType}, Fqbn: {Fqbn}, Name: {Name}, Serial: {Serial}, Type: {Type}, UserId: {UserId}, WifiFwVersion: {WifiFwVersion}",
                    device.ConnectionType, device.Fqbn, device.Name, device.Serial, device.Type, device.UserId, device.WifiFwVersion);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get devices: {Message}", ex.Message);
        }

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
                logger.LogInformation(
                    "ID: {Id}, Name: {Name}, Value: {Value}, Type: {Type}, Updated At: {ValueUpdatedAt}",
                    property.Id, property.Name, property.LastValue, property.Type, property.ValueUpdatedAt);
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
                logger.LogInformation("{Name} = {LastValue}", thingProperty.Name, thingProperty.LastValue);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to request the property: {Message}", ex.Message);
        }
    }
}