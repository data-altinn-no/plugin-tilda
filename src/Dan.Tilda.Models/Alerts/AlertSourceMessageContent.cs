using Newtonsoft.Json;

namespace Dan.Tilda.Models.Alerts;

[Serializable]
public class AlertSourceMessageContent
{
    [JsonProperty("meldingsType")]
    public string? MessageType { get; set; }

    [JsonProperty("fritekst")]
    public string? FreeText { get; set; }
}
