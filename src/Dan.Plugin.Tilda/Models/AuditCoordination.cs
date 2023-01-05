using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("tilsynskoordinering")]
    public class AuditCoordination
    {
        [JsonProperty("tildaenhet")]
        public string ControlObject;

        [JsonProperty("tilsynutfoertav", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlAgency;

        [JsonProperty("ansvarligtilsynsmyndighet")]
        public string ResponsibleAuditor;

        [JsonProperty("storulykketilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MajorAccidentAttributeType MajorAccidentAudit;

        [JsonProperty("internTilsynsid", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string InternalControlId;

        [JsonProperty("kontrolladresser", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AuditAddress> ControlLocations;

        [JsonProperty("kontaktpunkt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ControlContact> PlannedControlContact;

        [JsonProperty("meldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AlertFull> Alerts;

        [JsonProperty("aapnetilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int CurrentControls;

        [JsonProperty("planlagteKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<PlannedControlActivity> PlannedControlActivities;

        [JsonProperty("tilsynskampanjer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Campaign> ControlCampaigns;
    }
}
