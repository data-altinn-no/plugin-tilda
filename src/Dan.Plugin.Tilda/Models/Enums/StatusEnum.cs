using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatusEnum
    {
        [EnumMember(Value = "ok")]
        OK = 1,

        [EnumMember(Value = "feil")]
        Failed = 2,

        [EnumMember(Value = "ikkeFunnet")]
        NotFound = 3,

        [EnumMember(Value = "ukjent")]
        Unknown = 4
    }
}
