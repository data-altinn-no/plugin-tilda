using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("enhetsinformasjon")]
public class TildaRegistryEntry
{
    [JsonProperty("tildaenhet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? OrganizationNumber { get; set; }

    [JsonProperty("tildaenhetNavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Name { get; set; }

    [JsonProperty("epostaddresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<string>? Emails { get; set; }

    [JsonProperty("tildaenhetHovedenhet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlObjectParent { get; set; }

    [JsonProperty("besoeksadresse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ErAddress? PublicLocationAddress { get; set; }

    [JsonProperty("naeringskode", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? BusinessCode { get; set; }

    [JsonProperty("organisasjonsform", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? OrganisationForm { get; set; }

    [JsonProperty("regnskapsInformasjon", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public AccountsInformation? Accounts { get; set; }

    [JsonProperty("driftsstatus", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public OperationStatus OperationalStatus { get; set; }
}
