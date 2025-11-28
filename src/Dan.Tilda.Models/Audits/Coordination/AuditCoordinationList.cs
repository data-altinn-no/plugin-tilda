using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Coordination;

[JsonObject("tilsynskoordineringsliste")]
public class AuditCoordinationList : IAuditList
{
    [JsonProperty("status")]
    public StatusEnum Status { get; set; }

    [JsonProperty("statustekst")]
    public string StatusText { get; set; }

    [JsonProperty("tilsynsmyndighet")]
    public string ControlAgency { get; set; }

    [JsonProperty("tilsynskoordineringer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AuditCoordination> AuditCoordinations { get; set; }

    public AuditCoordinationList(string controlAgency)
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = controlAgency;
        AuditCoordinations = [];
    }

    public AuditCoordinationList()
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = string.Empty;
        AuditCoordinations = [];
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
