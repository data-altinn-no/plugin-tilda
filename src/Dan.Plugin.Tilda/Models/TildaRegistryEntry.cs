using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        public ERAddress PublicLocationAddress;

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
    }
}
