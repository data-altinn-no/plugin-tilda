using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Tilda.Models.Enums;

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
    Ok = 4,

    [EnumMember(Value = "slettet")]
    Slettet = 5
}
