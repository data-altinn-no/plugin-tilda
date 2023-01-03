using System;
using System.Collections.Generic;
using System.Text;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("trendrapportliste")]
    public class TrendReportList : IAuditList
    {
        [JsonProperty("status")]
        public StatusEnum Status;

        [JsonProperty("statustekst")]
        public string StatusText;

        [JsonProperty("tilsynsmyndighet")]
        public string ControlAgency;

        [JsonProperty("trendrapporter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<TrendReport> TrendReports;

        public TrendReportList(string controlAgency)
        {
            Status = StatusEnum.OK;
            StatusText = "";
            ControlAgency = controlAgency;
            TrendReports = new List<TrendReport>();
        }

        public TrendReportList()
        {

        }

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
