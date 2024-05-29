using Newtonsoft.Json;

namespace ArduinoCloudConnector;

public class ThingProperty(
    DateTime createdAt,
    string href,
    string id,
    object lastValue,
    bool linkedToTrigger,
    string name,
    string permission,
    bool persist,
    int tag,
    string thingId,
    string thingName,
    string type,
    int updateParameter,
    string updateStrategy,
    DateTime updatedAt,
    DateTime valueUpdatedAt,
    string variableName)
{
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; } = createdAt;

    [JsonProperty("href")] public string Href { get; set; } = href;

    [JsonProperty("id")] public string Id { get; set; } = id;

    [JsonProperty("last_value")] public object LastValue { get; set; } = lastValue;

    [JsonProperty("linked_to_trigger")] public bool LinkedToTrigger { get; set; } = linkedToTrigger;

    [JsonProperty("name")] public string Name { get; set; } = name;

    [JsonProperty("permission")] public string Permission { get; set; } = permission;

    [JsonProperty("persist")] public bool Persist { get; set; } = persist;

    [JsonProperty("tag")] public int Tag { get; set; } = tag;

    [JsonProperty("thing_id")] public string ThingId { get; set; } = thingId;

    [JsonProperty("thing_name")] public string ThingName { get; set; } = thingName;

    [JsonProperty("type")] public string Type { get; set; } = type;

    [JsonProperty("update_parameter")] public int UpdateParameter { get; set; } = updateParameter;

    [JsonProperty("update_strategy")] public string UpdateStrategy { get; set; } = updateStrategy;

    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; } = updatedAt;

    [JsonProperty("value_updated_at")] public DateTime ValueUpdatedAt { get; set; } = valueUpdatedAt;

    [JsonProperty("variable_name")] public string VariableName { get; set; } = variableName;
}

public class ThingPropertiesResponse(List<ThingProperty> properties)
{
    [JsonProperty("properties")] public List<ThingProperty> Properties { get; set; } = properties;
}