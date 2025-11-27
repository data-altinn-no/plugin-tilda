using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("tilsynsadresse")]
    public class ERAddress
    {
        [JsonProperty("lengdegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Latitude;

        [JsonProperty("breddegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Longtitude;

        [JsonProperty("bygningsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BuildingNumber;

        [JsonProperty("bruksenhetsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UnitNumber;

        [JsonProperty("adressenavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressName;

        [JsonProperty("adressenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressNumber;

        [JsonProperty("postnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostNumber;

        [JsonProperty("poststedsnavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostName;

        [JsonProperty("kommunenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MunicipalityNumber;

        [JsonProperty("bydel", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string District;

        [JsonProperty("fylkesnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CountyNumber;
    }
}
