using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.NPDID;

[JsonObject("npdidrapportliste")]
public class NpdidAuditReportList : IAuditList
{
    [JsonProperty("status")]
    public StatusEnum Status { get; set; }

    [JsonProperty("statustekst")]
    public string StatusText { get; set; }

    [JsonProperty("tilsynsmyndighet")]
    public string ControlAgency { get; set; }

    [JsonProperty("tilsynsrapporter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<NpdidAuditReport> AuditReports { get; set; }

    public NpdidAuditReportList(string controlAgency)
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = controlAgency;
        AuditReports = [];
    }

    public NpdidAuditReportList()
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = string.Empty;
        AuditReports = [];
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
