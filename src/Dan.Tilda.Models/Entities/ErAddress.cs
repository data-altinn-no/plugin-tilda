using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("tilsynsadresse")]
public class ErAddress
{
    [JsonProperty("breddegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Latitude { get; set; }

    [JsonProperty("lengdegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Longitude { get; set; }

    [JsonProperty("bygningsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? BuildingNumber { get; set; }

    [JsonProperty("bruksenhetsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? UnitNumber { get; set; }

    [JsonProperty("adressenavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? AddressName { get; set; }

    [JsonProperty("adressenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? AddressNumber { get; set; }

    [JsonProperty("postnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? PostNumber { get; set; }

    [JsonProperty("poststedsnavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? PostName { get; set; }

    [JsonProperty("kommunenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? MunicipalityNumber { get; set; }

    [JsonProperty("bydel", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? District { get; set; }

    [JsonProperty("fylkesnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? CountyNumber { get; set; }
}

