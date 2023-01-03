using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dan.Plugin.Tilda.Models
{
    [JsonObject("StorulykkevirksomhetKontroll")]
    internal class StorulykkevirksomhetKontroll
    {
        [JsonProperty("bedriftsnummer")]
        public string OrganizationNumber;

        [JsonProperty("paragraf6")]
        public bool Paragraph6;

        [JsonProperty("paragraf9")]
        public bool Paragraph9;
    }
}
