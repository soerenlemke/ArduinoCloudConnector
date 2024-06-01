using Newtonsoft.Json;

namespace ArduinoCloudConnector.Models;

public class Thing(
    string deviceId,
    string id,
    string name,
    string properties,
    string tags,
    string timezone,
    string webhookActive,
    string webhookUri)
{
    [JsonProperty("device_id")] public string DeviceId { get; set; } = deviceId;
    [JsonProperty("id")] public string Id { get; set; } = id;
    [JsonProperty("name")] public string Name { get; set; } = name;
    [JsonProperty("properties")] public string Properties { get; set; } = properties;
    [JsonProperty("tags")] public string Tags { get; set; } = tags;
    [JsonProperty("timezone")] public string Timezone { get; set; } = timezone;
    [JsonProperty("webhook_active")] public string WebhookActive { get; set; } = webhookActive;
    [JsonProperty("webhook_uri")] public string WebhookUri { get; set; } = webhookUri;
}