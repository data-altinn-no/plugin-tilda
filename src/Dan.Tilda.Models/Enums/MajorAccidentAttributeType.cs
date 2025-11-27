using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Tilda.Models.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum MajorAccidentAttributeType
{
    [EnumMember(Value = "ja")]
    Ja = 4,
    [EnumMember(Value = "nei")]
    Nei = 1,
    [EnumMember(Value = "meldepliktig")]
    Meldepliktig = 2,
    [EnumMember(Value = "rapporteringspliktig")]
    Rapporteringspliktig = 3,
    [EnumMember(Value = "ikkeAngitt")]
    Blank = 0,
}
