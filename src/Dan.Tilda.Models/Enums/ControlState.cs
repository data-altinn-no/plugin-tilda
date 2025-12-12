using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Tilda.Models.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ControlState
{
    [EnumMember(Value = "planlegging")]
    Planlegging = 4,

    [EnumMember(Value = "aapen")]
    Aapen = 1,

    [EnumMember(Value = "lukket")]
    Lukket = 2,

    [EnumMember(Value = "avbrutt")]
    Avbrutt = 3,

    [EnumMember(Value = "ikkeAngitt")]
    Blank = 0,
}
