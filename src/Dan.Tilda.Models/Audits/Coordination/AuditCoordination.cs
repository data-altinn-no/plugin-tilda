using Dan.Tilda.Models.Entities;
using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.Coordination;

[JsonObject("tilsynskoordinering")]
public class AuditCoordination
{
    [JsonProperty("tildaenhet")]
    public string? ControlObject { get; set; }

    [JsonProperty("tilsynutfoertav", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlAgency { get; set; }

    [JsonProperty("ansvarligtilsynsmyndighet")]
    public string? ResponsibleAuditor { get; set; }

    [JsonProperty("tilsynsstatus")]
    public ControlState ControlPlanningStatus { get; set; }

    [JsonProperty("uanmeldttilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public SurpriseControlAttributeType NotNotified { get; set; }

    [JsonProperty("storulykketilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public MajorAccidentAttributeType MajorAccidentAudit { get; set; }

    [JsonProperty("internTilsynsid", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? InternalControlId { get; set; }

    [JsonProperty("kontrolladresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AuditAddress>? ControlLocations { get; set; }

    [JsonProperty("kontaktpunkt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<ControlContact>? PlannedControlContact { get; set; }

    [JsonProperty("meldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AlertFull>? Alerts { get; set; }

    [JsonProperty("aapnetilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int CurrentControls { get; set; }

    [JsonProperty("planlagteKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<PlannedControlActivity>? PlannedControlActivities { get; set; }

    [JsonProperty("tilsynskampanjer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<Campaign>? ControlCampaigns { get; set; }
}
