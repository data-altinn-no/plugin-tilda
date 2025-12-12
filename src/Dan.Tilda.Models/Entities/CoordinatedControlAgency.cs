using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("samtidigKontroll")]
public class CoordinatedControlAgency
{
    [JsonProperty("samtidigTilsynsmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlAgency { get; set; }

    [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlTopic { get; set; }

    [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ActivityExecution { get; set; }
}
