using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("meldingTilAnnenMyndighet")]
public class AlertCompact
{
    [JsonProperty("meldingTilmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ReceivingControlAgency { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Message { get; set; }
}
