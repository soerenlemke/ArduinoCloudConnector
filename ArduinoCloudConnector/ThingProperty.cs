using Newtonsoft.Json;

namespace ArduinoCloudConnector;

public class ThingProperty
{
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("href")]
    public string Href { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("last_value")]
    public object LastValue { get; set; }

    [JsonProperty("linked_to_trigger")]
    public bool LinkedToTrigger { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("permission")]
    public string Permission { get; set; }

    [JsonProperty("persist")]
    public bool Persist { get; set; }

    [JsonProperty("tag")]
    public int Tag { get; set; }

    [JsonProperty("thing_id")]
    public string ThingId { get; set; }

    [JsonProperty("thing_name")]
    public string ThingName { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("update_parameter")]
    public int UpdateParameter { get; set; }

    [JsonProperty("update_strategy")]
    public string UpdateStrategy { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("value_updated_at")]
    public DateTime ValueUpdatedAt { get; set; }

    [JsonProperty("variable_name")]
    public string VariableName { get; set; }
}

public class ThingPropertiesResponse
{
    [JsonProperty("properties")]
    public List<ThingProperty> Properties { get; set; }
}