using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Storulykke;

[JsonObject("storulykkevirksomhetListe")]
public class StorulykkevirksomhetListe
{
    [JsonProperty("storulykkevirksomheter")]
    public List<string>? Organizations { get; set; }
}
