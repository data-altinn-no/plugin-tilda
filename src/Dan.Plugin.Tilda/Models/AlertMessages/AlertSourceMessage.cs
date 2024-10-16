using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models.AlertMessages;

[Serializable]
public class AlertSourceMessage
{
    [JsonProperty("identifikator")]
    public string Id { get; set; }

    [JsonProperty("mottaker")]
    public string Recipient { get; set; }

    [JsonProperty("meldingOmTildaenhet")]
    public string Subject { get; set; }

    [JsonProperty("datoForMeldingTilAnnenMyndighet")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet")]
    public AlertSourceMessageContent MessageContent { get; set; }
}

[Serializable]
public class AlertSourceMessageContent
{
    [JsonProperty("meldingsType")]
    public string MessageType { get; set; }

    [JsonProperty("fritekst")]
    public string FreeText { get; set; }
}
