using Newtonsoft.Json;

namespace Dan.Tilda.Models.Alerts;

[Serializable]
public class AlertSourceMessage
{
    [JsonProperty("identifikator")]
    public string? Id { get; set; }

    [JsonProperty("mottaker")]
    public string? Recipient { get; set; }

    [JsonProperty("meldingOmTildaenhet")]
    public string? Subject { get; set; }

    [JsonProperty("datoForMeldingTilAnnenMyndighet")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet")]
    public AlertSourceMessageContent? MessageContent { get; set; }
}
