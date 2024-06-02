using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ArduinoCloudConnector.Models;

public class Device(
    ConnectionType connectionType,
    string fqbn,
    string name,
    string serial,
    DeviceType deviceType,
    string userId,
    string wifiFwVersion)
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("connection_type")]
    public ConnectionType ConnectionType { get; set; } = connectionType;
    
    [JsonProperty("fqbn")] public string Fqbn { get; set; } = fqbn;

    [JsonProperty("name")] [MaxLength(64)] public string Name { get; set; } = name;

    [JsonProperty("serial")] [MaxLength(64)] public string Serial { get; set; } = serial;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("type")]
    public DeviceType Type { get; set; } = deviceType;

    [JsonProperty("user_id")] public string UserId { get; set; } = userId;

    [JsonProperty("wifi_fw_version")]
    [MaxLength(10)]
    public string WifiFwVersion { get; set; } = wifiFwVersion;
}

public enum ConnectionType
{
    Wifi,
    Eth,
    WifiAndSecret,
    Gsm,
    Nb,
    Lora
}

public enum DeviceType
{
    Mkrwifi1010,
    Mkr1000,
    Nano_33_iot,
    Mkrgsm1400,
    Mkrwan1310,
    Mkrwan1300,
    Mkrnb1500,
    [EnumMember(Value = "lora-device")] LoraDevice,
    Login_and_secretkey_wifi,
    Envie_m7,
    Nanorp2040connect,
    Nicla_vision,
    Phone,
    Portenta_x8,
    Opta,
    Giga,
    Generic_device_secretkey,
    Portenta_c33,
    Unor4wifi,
    Nano_nora
}