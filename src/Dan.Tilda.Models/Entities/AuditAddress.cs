using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("tilsynsadresse")]
public class AuditAddress : ErAddress
{
    [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int LocationReference { get; set; }

    [JsonProperty("lokalitetsbeskrivelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? LocationDescription { get; set; }

    [JsonProperty("lokalitetsnoekkelord", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? LocationKeywords { get; set; }
}
