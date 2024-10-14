using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models.AlertMessages;

public class AlertMessage
{
    [JsonProperty("identifikator")]
    public string Id { get; set; }

    [JsonProperty("meldingFraMyndighet")]
    public string Source { get; set; }

    [JsonProperty("meldingOmTildaenhet")]
    public string Subject { get; set; }

    [JsonProperty("datoForMeldingTilAnnenMyndighet")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("meldingsinnholdTilAnnenMyndighet")]
    public AlertMessageContent MessageContent { get; set; }
}

[Serializable]
public class AlertMessageContent
{
    [JsonProperty("meldingsType")]
    public string MessageType { get; set; }

    [JsonProperty("relatertDatasettOppslagsUrl")]
    public string DatasetUrl { get; set; }

    [JsonProperty("fritekst")]
    public string FreeText { get; set; }
}
