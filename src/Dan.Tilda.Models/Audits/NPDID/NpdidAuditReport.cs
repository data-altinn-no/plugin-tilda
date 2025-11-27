using Dan.Tilda.Models.Entities;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Audits.NPDID;

[JsonObject("NPDID")]
public class NpdidAuditReport
{
    [JsonProperty("npdid")]
    public string? Npdid { get; set; }

    [JsonProperty("tildaenhet")]
    public string? ControlObject { get; set; }

    [JsonProperty("tilsynutfoertav")]
    public string? ControlAgency { get; set; }

    [JsonProperty("ansvarligtilsynsmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ResponsibleAuditor { get; set; }

    [JsonProperty("tilsynsegenskaper", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ControlAttribute? ControlAttributes { get; set; }

    [JsonProperty("kontrolladresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<AuditAddress>? ControlLocations { get; set; }

    [JsonProperty("utfoerteTilsynsaktiviteter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<ControlActivity>? ControlActivities { get; set; }

    [JsonProperty("kontaktpunkt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<ControlContact>? ControlContacts { get; set; }

    [JsonProperty("tilsynsnotater", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? AuditNotes { get; set; }

    [JsonProperty("anmerkninger", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<Remark>? NotesAndRemarks { get; set; }

    [JsonProperty("bruddOgReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<Reaction>? ViolationReactions { get; set; }
}
