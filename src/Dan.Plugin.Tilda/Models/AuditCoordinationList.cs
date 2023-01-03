using System;
using System.Collections.Generic;
using System.Text;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("tilsynskoordineringsliste")]
    public class AuditCoordinationList : IAuditList
    {
        [JsonProperty("status")]
        public StatusEnum Status;

        [JsonProperty("statustekst")]
        public string StatusText;

        [JsonProperty("tilsynsmyndighet")]
        public string ControlAgency;

        [JsonProperty("tilsynskoordineringer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AuditCoordination> AuditCoordinations;

        public AuditCoordinationList(string controlAgency)
        {
            Status = StatusEnum.OK;
            StatusText = "";
            ControlAgency = controlAgency;
            AuditCoordinations = new List<AuditCoordination>();
        }

        public AuditCoordinationList()
        {}

        public void SetStatusAndTextAndOwner(string statusText, StatusEnum status, string owner)
        {
            Status = status;
            StatusText = statusText;
            ControlAgency = owner;
        }

        public string GetOwner()
        {
            return ControlAgency;
        }
    }
}
