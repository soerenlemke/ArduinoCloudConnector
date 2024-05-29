using DotNetEnv;

namespace ArduinoCloudConnector.ConsoleApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();
            var clientId = Env.GetString("CLIENT_ID");
            var clientSecret = Env.GetString("CLIENT_SECRET");

            var arduinoCloudClient = new ArduinoCloudClient(clientId, clientSecret);
            try
            {
                var accessToken = await arduinoCloudClient.GetAccessTokenAsync();
                Console.WriteLine(accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get access token: {ex.Message}");
            }
        }
    }
}