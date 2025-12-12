using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("meldingTilAnnenMyndighet")]
public class AlertFull
{
    [JsonProperty("meldingTilMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ReceivingControlAgency { get; set; }

    [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int LocationReference { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Message { get; set; }

    [JsonProperty("datoForMeldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime Date { get; set; }
}
