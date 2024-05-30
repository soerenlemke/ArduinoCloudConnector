using ArduinoCloudConnector.Models;
using ArduinoCloudConnector.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArduinoCloudConnector.Console;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient<ArduinoCloudClient>();
                services.Configure<ArduinoCloudClientOptions>(options =>
                {
                    options.ClientId = Env.GetString("CLIENT_ID");
                    options.ClientSecret = Env.GetString("CLIENT_SECRET");
                });
            })
            .Build();

        Env.Load();
        var thingId = Env.GetString("THING_ID");

        var arduinoCloudClient = host.Services.GetRequiredService<ArduinoCloudClient>();
        try
        {
            var thingProperties = await arduinoCloudClient.GetThingPropertiesAsync(thingId);
            if (thingProperties is null)
            {
                System.Console.WriteLine($"No properties for the Thing: {thingId}");
                return;
            }

            foreach (var property in thingProperties)
                System.Console.WriteLine(
                    $"Name: {property.Name}, Value: {property.LastValue}, Type: {property.Type}, Updated At: {property.ValueUpdatedAt}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to get thing properties: {ex.Message}");
        }
    }
}