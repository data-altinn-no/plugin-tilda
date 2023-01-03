using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("npdidrapportliste")]
    public class NPDIDAuditReportList : IAuditList
    {
        [JsonProperty("status")]
        public StatusEnum Status;

        [JsonProperty("statustekst")]
        public string StatusText;

        [JsonProperty("tilsynsmyndighet")]
        public string ControlAgency;

        [JsonProperty("tilsynsrapporter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<NPDIDAuditReport> AuditReports;

        public NPDIDAuditReportList(string controlAgency)
        {
            Status = StatusEnum.OK;
            StatusText = "";
            ControlAgency = controlAgency;
            AuditReports = new List<NPDIDAuditReport>();
        }

        public NPDIDAuditReportList()
        { }

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
