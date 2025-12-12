using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Tilda.Models.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum StatusEnum
{
    [EnumMember(Value = "ok")]
    Ok = 1,

    [EnumMember(Value = "feil")]
    Failed = 2,

    [EnumMember(Value = "ikkeFunnet")]
    NotFound = 3,

    [EnumMember(Value = "ukjent")]
    Unknown = 4
}
