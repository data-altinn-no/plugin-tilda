using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationStatus
    {
        [EnumMember(Value = "ikkeAngitt")]
        Blank = 0,

        [EnumMember(Value = "konkurs")]
        Konkurs = 1,

        [EnumMember(Value = "underAvvikling")]
        UnderAvvikling = 2,

        [EnumMember(Value = "underTvangsavviklingEllerTvangsopploesning")]
        UnderTvangsavviklingEllerTvangsopplosning = 3,

        [EnumMember(Value = "ok")]
        OK = 4,

        [EnumMember(Value = "slettet")]
        Slettet = 5
    }

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
