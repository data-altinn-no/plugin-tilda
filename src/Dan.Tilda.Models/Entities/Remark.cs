using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("anmerkning")]
public class Remark
{
    [JsonProperty("anmerkningsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Remarkreference { get; set; }

    [JsonProperty("anmerkning", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? RemarkMessage { get; set; }
}
