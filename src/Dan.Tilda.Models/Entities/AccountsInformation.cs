using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

public class AccountsInformation
{
    [JsonProperty("fraDato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? FromDate { get; set; }

    [JsonProperty("tilDato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ToDate { get; set; }

    [JsonProperty("aarligOmsetning", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? AnnualTurnover { get; set; }

    [JsonProperty("driftsresultat", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? AnnualResult { get; set; }

    [JsonProperty("sumEgenkapital", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? TotalEquity { get; set; }

    [JsonProperty("sumGjeld", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? TotalDebt { get; set; }

    [JsonProperty("sumKortsiktigGjeld", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ShortTermDebt { get; set; }

    [JsonProperty("omloepsmidler", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? CurrentAssets { get; set; }

    [JsonProperty("opptjentEgenkapital", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? EarnedEquity { get; set; }

}
