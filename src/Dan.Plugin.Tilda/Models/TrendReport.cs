using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("trendrapport")]
    public class TrendReport
    {
        [JsonProperty("tildaenhet")]
        public string ControlObject;

        [JsonProperty("ansvarligtilsynsmyndighet")]
        public string ResponsibleAuditor;

        [JsonProperty("aarligeTildaenhetTotaler", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Annual> AnnualTotals;

    }

    [JsonObject("aarligTotal")]
    public class Annual
    {
        [JsonProperty("trenddataForKalenderAar")]
        public int Year;

        [JsonProperty("antallMeldingerTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberofAlerts;

        [JsonProperty("antallMaanederMedData", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MonthsOfData;

        [JsonProperty("antallAnmerkninger", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfRemarks;

        [JsonProperty("antallTilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfAudits;

        [JsonProperty("antallTilsynUtenReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfAuditsWithoutReactions;

        [JsonProperty("antallTilsynMedReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfAuditsWithReactions;

        [JsonProperty("antallKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfControls;

        [JsonProperty("antallKontrollerUtenReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfControlsWithoutReactions;

        [JsonProperty("antallKontrollerMedReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfControlsWithReactions;

        [JsonProperty("antallAnmeldteReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int PoliceReactions;
    }
}
