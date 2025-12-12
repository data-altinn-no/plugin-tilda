using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Trend;

[JsonObject("aarligTotal")]
public class Annual
{
    [JsonProperty("trenddataForKalenderAar")]
    public int Year { get; set; }

    [JsonProperty("antallMeldingerTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfAlerts { get; set; }

    [JsonProperty("antallMaanederMedData", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int MonthsOfData { get; set; }

    [JsonProperty("antallAnmerkninger", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfRemarks { get; set; }

    [JsonProperty("antallTilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfAudits { get; set; }

    [JsonProperty("antallTilsynUtenReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfAuditsWithoutReactions { get; set; }

    [JsonProperty("antallTilsynMedReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfAuditsWithReactions { get; set; }

    [JsonProperty("antallKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfControls { get; set; }

    [JsonProperty("antallKontrollerUtenReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfControlsWithoutReactions { get; set; }

    [JsonProperty("antallKontrollerMedReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfControlsWithReactions { get; set; }

    [JsonProperty("antallAnmeldteReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int PoliceReactions { get; set; }
}
