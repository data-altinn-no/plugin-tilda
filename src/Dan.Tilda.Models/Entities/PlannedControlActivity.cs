using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("planlagtekontroller")]
public class PlannedControlActivity
{
    [JsonProperty("planlagtkontrolldato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime Date { get; set; }

    [JsonProperty("planlagtkontrollVarighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Days { get; set; }

    [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Topic { get; set; }

    [JsonProperty("tilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Activity { get; set; }

    [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ActivityExecutionType { get; set; }

    [JsonProperty("samtidigeKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<CoordinatedControlAgency>? CoordinatedControl { get; set; }
}
