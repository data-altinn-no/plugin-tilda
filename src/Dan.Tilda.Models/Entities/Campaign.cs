using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("kampanje")]
public class Campaign
{
    [JsonProperty("kampanjenavn")]
    public string? Name { get; set; }

    [JsonProperty("kampanjebeskrivelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Description { get; set; }

    [JsonProperty("startdatoForKampanje", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime StartDate { get; set; }

    [JsonProperty("sluttdatoForKampanje", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime EndDate { get; set; }
}
