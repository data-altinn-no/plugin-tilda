using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Report;

[JsonObject("tilsynsrapportliste")]
public class AuditReportList : IAuditList
{
    [JsonProperty("status")]
    public StatusEnum Status { get; set; }

    [JsonProperty("statustekst")]
    public string StatusText { get; set; }

    [JsonProperty("tilsynsmyndighet")]
    public string ControlAgency { get; set; }

    [JsonProperty("tilsynsrapporter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AuditReport>? AuditReports { get; set; }

    public AuditReportList(string controlAgency)
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = controlAgency;
        AuditReports = [];
    }

    public AuditReportList()
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
