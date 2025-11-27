using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("tilsynsaktivitet")]
public class ControlActivity
{
    [JsonProperty("tilsynsaktivitetreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ActivityReference { get; set; }

    [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int LocationReference { get; set; }

    [JsonProperty("internAktivitetsidentifikator", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? InternalControlId { get; set; }

    [JsonProperty("kontrollobjekt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlObject { get; set; }

    [JsonProperty("startdatoForTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime Date { get; set; }

    [JsonProperty("varighetForTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Days { get; set; }

    [JsonProperty("tilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Activity { get; set; }

    [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ActivityExecutionType { get; set; }

    [JsonProperty("observasjonFraTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Observation { get; set; }

    [JsonProperty("samtidigeKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<CoordinatedControlAgency>? CoordinatedControl { get; set; }

    [JsonProperty("meldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AlertCompact>? AlertMessages { get; set; }

}
