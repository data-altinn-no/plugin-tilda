using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("kontaktpunkt")]
public class ControlContact
{
    [JsonProperty("kontaktperson", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ResponsibleName { get; set; }

    [JsonProperty("avdeling", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Department { get; set; }

    [JsonProperty("telefonnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? PhoneNumber { get; set; }

    [JsonProperty("epost", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Email { get; set; }

    [JsonProperty("adresse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Address { get; set; }
}
