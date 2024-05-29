using System;
using System.Threading.Tasks;
using DotNetEnv;

namespace ArduinoCloudConnector.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();
            var clientId = Env.GetString("CLIENT_ID");
            var clientSecret = Env.GetString("CLIENT_SECRET");
            var thingId = Env.GetString("THING_ID");

            var arduinoCloudClient = new ArduinoCloudClient(clientId, clientSecret);
            try
            {
                var thingProperties = await arduinoCloudClient.GetThingPropertiesAsync(thingId);

                foreach (var property in thingProperties)
                {
                    System.Console.WriteLine($"Name: {property.Name}, Value: {property.LastValue}, Type: {property.Type}, Updated At: {property.ValueUpdatedAt}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to get thing properties: {ex.Message}");
            }
        }
    }
}