using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("NPDID")]
    public class NPDIDAuditReport
    {
        [JsonProperty("npdid")]
        public string Npdid;

        [JsonProperty("tildaenhet")]
        public string ControlObject;

        [JsonProperty("tilsynutfoertav")]
        public string ControlAgency;

        [JsonProperty("ansvarligtilsynsmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ResponsibleAuditor;

        [JsonProperty("tilsynsegenskaper", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ControlAttribute ControlAttributes;

        [JsonProperty("kontrolladresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AuditAddress> ControlLocations;

        [JsonProperty("utfoerteTilsynsaktiviteter", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ControlActivity> ControlActivities;

        [JsonProperty("kontaktpunkt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ControlContact> ControlContacts;

        [JsonProperty("tilsynsnotater", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AuditNotes;

        [JsonProperty("anmerkninger", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Remark> NotesAndRemarks;

        [JsonProperty("bruddOgReaksjoner", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Reaction> ViolationReactions;
    }
}
