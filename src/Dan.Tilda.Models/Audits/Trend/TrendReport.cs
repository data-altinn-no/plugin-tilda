using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Trend;

[JsonObject("trendrapport")]
public class TrendReport
{
    [JsonProperty("tildaenhet")]
    public string? ControlObject { get; set; }

    [JsonProperty("ansvarligtilsynsmyndighet")]
    public string? ResponsibleAuditor { get; set; }

    [JsonProperty("aarligeTildaenhetTotaler", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<Annual>? AnnualTotals { get; set; }

}
