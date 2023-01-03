using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{
    public class Audit
    {
        [JsonProperty("orgNo")]
        public string OrgNo { get; set; }

        [JsonProperty("orgName")]
        public string OrgName { get; set; }

        [JsonProperty("audits")]
        public List<AuditElement> Audits { get; set; }
    }
}
