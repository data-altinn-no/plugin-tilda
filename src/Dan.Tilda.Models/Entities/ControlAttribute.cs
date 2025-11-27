using Dan.Tilda.Models.Enums;
using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("tilsynsegenskap")]
public class ControlAttribute
{
    [JsonProperty("internTilsynsid", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? InternalControlId { get; set; }

    [JsonProperty("storulykketilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public MajorAccidentAttributeType Major { get; set; }

    [JsonProperty("uanmeldttilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
    public SurpriseControlAttributeType NotNotified { get; set; }

    [JsonProperty("tilsynsutvelgelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? SelectionCriteria { get; set; }

    [JsonProperty("tilsynsstatus", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ControlState ControlStatus { get; set; }

    [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlTopic { get; set; }

    [JsonProperty("tilsynsnoekkelord", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ControlKeywords { get; set; }

    [JsonProperty("nettrapport", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? WebReportUrl { get; set; }
}
