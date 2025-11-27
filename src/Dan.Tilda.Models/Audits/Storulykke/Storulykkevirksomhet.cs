using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Storulykke;

[JsonObject("storulykkevirksomhet")]
public class Storulykkevirksomhet
{
    [JsonProperty("bedriftsnummer")]
    public string? OrganizationNumber { get; set; }

    [JsonProperty("navn")]
    public string? Name { get; set; }
}
