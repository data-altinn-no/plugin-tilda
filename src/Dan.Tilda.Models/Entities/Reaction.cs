using Newtonsoft.Json;

namespace Dan.Tilda.Models.Entities;

[JsonObject("bruddOgReaksjon")]
public class Reaction
{
    [JsonProperty("bruddOgReaksjonsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ReactionReference { get; set; }

    [JsonProperty("tilsynsaktivitetreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ControlActivityReference { get; set; }

    [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int ControlLocationReference { get; set; }

    [JsonProperty("utredningAvBruddOgReaksjon", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Explanation { get; set; }

    [JsonProperty("lovparagraf", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Paragraph { get; set; }

    [JsonProperty("reaksjonsdato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime ReactionDate { get; set; }

    [JsonProperty("alvorsgrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ControlReactionDetails? ControlReactionsDetails { get; set; }

    [JsonProperty("antallGangerVirkemiddelErTattIBruk", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int NumberOfEffectuatedReactions { get; set; }

}
