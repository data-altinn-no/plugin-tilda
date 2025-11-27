using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Storulykke;

[JsonObject("StorulykkevirksomhetKontroll")]
public class StorulykkevirksomhetKontroll
{
    [JsonProperty("bedriftsnummer")]
    public string? OrganizationNumber { get; set; }

    [JsonProperty("paragraf6")]
    public bool Paragraph6 { get; set; }

    [JsonProperty("paragraf9")]
    public bool Paragraph9 { get; set; }
}
