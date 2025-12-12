using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Tilda.Models.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SurpriseControlAttributeType
{
    [EnumMember(Value = "ja")]
    Ja = 1,
    [EnumMember(Value = "nei")]
    Nei = 2,
    [EnumMember(Value = "ikkeAngitt")]
    Blank = 0
}
