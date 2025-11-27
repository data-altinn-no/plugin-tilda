using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Alerts;

[JsonObject("meldingTilAnnenMyndighet")]
public class AlertMessage
{
    [JsonProperty("meldingFraMyndighet")]
    public string? AuditingAgency { get; set; }

    [JsonProperty("meldingOmTildaenhet")]
    public string? ControlObject { get; set; }

    [JsonProperty("datoForMeldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime AlertDate { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Message { get; set; }

    [JsonProperty("identifikator", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Id { get; set; }
}
