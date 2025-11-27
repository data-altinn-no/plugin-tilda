using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Alerts;

[JsonObject("TildaMeldingTilAnnenMyndighet")]
public class AlertMessageList : IAuditList
{
    [JsonProperty("status")]
    public StatusEnum Status;

    [JsonProperty("statustekst")]
    public string StatusText;

    [JsonProperty("tilsynsmyndighet")]
    public string ControlAgency;

    [JsonProperty("meldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AlertMessage> AlertMessages;

    public AlertMessageList(string controlAgency)
    {
        Status = StatusEnum.Ok;
        StatusText = string.Empty;
        ControlAgency = controlAgency;
        AlertMessages = [];
    }

    public AlertMessageList()
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
