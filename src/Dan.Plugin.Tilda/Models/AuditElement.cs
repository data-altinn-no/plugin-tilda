using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Models
{
    using Newtonsoft.Json;

    public class AuditElement
    {
        [JsonProperty("auditDate")]
        public DateTime AuditDate { get; set; }

        [JsonProperty("auditResult")]
        public string AuditResult { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("knownAudit")]
        public bool KnownAudit { get; set; }

        [JsonProperty("auditor")]
        public string Auditor { get; set; }

        [JsonProperty("locationOrg", NullValueHandling = NullValueHandling.Ignore)]
        public string LocationOrg { get; set; }
    }
}
