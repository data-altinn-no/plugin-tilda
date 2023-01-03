using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("tilsynsrapport")]
    public class AuditReport
    {
        [JsonProperty("tildaenhet")]
        public string ControlObject;

        [JsonProperty("tilsynutfoertav", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlAgency;

        [JsonProperty("ansvarligTilsynsmyndighet")]
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
