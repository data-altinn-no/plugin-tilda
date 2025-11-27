using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Trend;

[JsonObject("trendrapportliste")]
public class TrendReportList : IAuditList
{
    [JsonProperty("status")]
    public StatusEnum Status { get; set; }

    [JsonProperty("statustekst")]
    public string StatusText { get; set; }

    [JsonProperty("tilsynsmyndighet")]
    public string ControlAgency { get; set; }

    [JsonProperty("trendrapporter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<TrendReport> TrendReports { get; set; }

    public TrendReportList(string controlAgency)
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = controlAgency;
        TrendReports = [];
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
