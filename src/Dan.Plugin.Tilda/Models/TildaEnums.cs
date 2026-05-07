using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlTopic
    {
        [JsonProperty("hendelse")]
        Hendelse = 1,

        [JsonProperty("proevetaking")]
        ProeveTaking = 2,

        [JsonProperty("utslippTilLuft")]
        UtslippTilLuft = 3,

        [JsonProperty("utslippTilVann")]
        UtslippTilVann = 4

    }
}
