using Dan.Tilda.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("enhetsinformasjon")]
    public class TildaRegistryEntry
    {
        [JsonProperty("tildaenhet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OrganizationNumber;

        [JsonProperty("tildaenhetNavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name;

        [JsonProperty("epostaddresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Emails;

        [JsonProperty("tildaenhetHovedenhet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlObjectParent;

        [JsonProperty("besoeksadresse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ErAddress PublicLocationAddress;

        [JsonProperty("naeringskode", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BusinessCode;

        [JsonProperty("organisasjonsform", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OrganisationForm;

        [JsonProperty("regnskapsInformasjon", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccountsInformation Accounts;

        [JsonProperty("driftsstatus", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OperationStatus OperationalStatus;
    }

    public class AccountsInformation
    {
        [JsonProperty("fraDato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime FromDate;

        [JsonProperty("tilDato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ToDate;

        [JsonProperty("aarligOmsetning", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AnnualTurnover;

        [JsonProperty("driftsresultat", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AnnualResult;

        [JsonProperty("sumEgenkapital", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TotalEquity;

        [JsonProperty("sumGjeld", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TotalDebt;

        [JsonProperty("sumKortsiktigGjeld", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ShortTermDebt;

        [JsonProperty("omloepsmidler", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CurrentAssets;

        [JsonProperty("opptjentEgenkapital", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string EarnedEquity;

    }
}
