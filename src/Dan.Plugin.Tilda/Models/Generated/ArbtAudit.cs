namespace Dan.Plugin.Tilda.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ArbtAuditElement
    {
        [JsonProperty("Orgnummer", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Orgnummer { get; set; }

        [JsonProperty("Dato", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Dato { get; set; }

        [JsonProperty("Sted", NullValueHandling = NullValueHandling.Ignore)]
        public string Sted { get; set; }

        [JsonProperty("Kilde", NullValueHandling = NullValueHandling.Ignore)]
        public string Kilde { get; set; }
    }

    public class ArbtAudit : List<ArbtAuditElement>
    {

    }
}
