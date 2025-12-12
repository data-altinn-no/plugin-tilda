using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("alvorsgrad")]
public class ControlReactionDetails
{
    [JsonProperty("utmaaltReaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Value { get; set; }

    [JsonProperty("utmaaltReaksjonstype", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ReactionType { get; set; }

    [JsonProperty("utmaaltReaksjonsklasse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ReactionClass { get; set; }

    [JsonProperty("lavreaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ReactionLow { get; set; }

    [JsonProperty("hoeyreaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ReactionHigh { get; set; }

    [JsonProperty("lavalvorsgradindeks", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int DegreeLow { get; set; }

    [JsonProperty("hoeyalvorsgradindeks", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int DegreeHigh { get; set; }
}
