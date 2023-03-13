using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("storulykkevirksomhetListe")]
    public class StorulykkevirksomhetListe
    {
        [JsonProperty("storulykkevirksomheter")]
        public string[] Organizations;
    }
}
