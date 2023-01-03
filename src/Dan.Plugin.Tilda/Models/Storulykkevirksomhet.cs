using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("storulykkevirksomhet")]
    public class Storulykkevirksomhet
    {
        [JsonProperty("bedriftsnummer")]
        public string OrganizationNumber;

        [JsonProperty("navn")]
        public string Name;
    }
}
