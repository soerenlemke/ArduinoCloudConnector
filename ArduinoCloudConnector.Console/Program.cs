using DotNetEnv;

namespace ArduinoCloudConnector.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // Laden der Umgebungsvariablen aus der .env Datei
            Env.Load();
            var clientId = Env.GetString("CLIENT_ID");
            var clientSecret = Env.GetString("CLIENT_SECRET");
            var thingId = Env.GetString("THING_ID");

            var arduinoCloudClient = new ArduinoCloudClient(clientId, clientSecret);
            try
            {
                var thingProperties = await arduinoCloudClient.GetThingPropertiesAsync(thingId);
                System.Console.WriteLine($"Thing Properties: {thingProperties}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to get thing properties: {ex.Message}");
            }
        }
    }
}