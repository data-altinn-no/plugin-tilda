namespace Dan.Plugin.Tilda.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class JsvAudit
    {
        [JsonProperty("aktorid", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Aktorid { get; set; }

        [JsonProperty("aktororgnr", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Aktororgnr { get; set; }

        [JsonProperty("aktornavn", NullValueHandling = NullValueHandling.Ignore)]
        public string Aktornavn { get; set; }

        [JsonProperty("aktorstatus", NullValueHandling = NullValueHandling.Ignore)]
        public string Aktorstatus { get; set; }

        [JsonProperty("aktorrisiko", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Aktorrisiko { get; set; }

        [JsonProperty("lokasjoner", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Lokasjoner> Lokasjoner { get; set; }
    }

    public partial class Lokasjoner
    {
        [JsonProperty("aktorid", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Aktorid { get; set; }

        [JsonProperty("aktororgnr", NullValueHandling = NullValueHandling.Ignore)]
        public string Aktororgnr { get; set; }

        [JsonProperty("aktornavn", NullValueHandling = NullValueHandling.Ignore)]
        public string Aktornavn { get; set; }

        [JsonProperty("adresse", NullValueHandling = NullValueHandling.Ignore)]
        public string Adresse { get; set; }

        [JsonProperty("adressepostnummer", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Adressepostnummer { get; set; }

        [JsonProperty("adressested", NullValueHandling = NullValueHandling.Ignore)]
        public string Adressested { get; set; }

        [JsonProperty("adressegeolat", NullValueHandling = NullValueHandling.Ignore)]
        public string Adressegeolat { get; set; }

        [JsonProperty("adressegeolong", NullValueHandling = NullValueHandling.Ignore)]
        public string Adressegeolong { get; set; }

        [JsonProperty("aktorstatus", NullValueHandling = NullValueHandling.Ignore)]
        public string Aktorstatus { get; set; }

        [JsonProperty("aktorrisiko", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Aktorrisiko { get; set; }

        [JsonProperty("instrumenter", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Instrumenter { get; set; }

        [JsonProperty("tilsynsist", NullValueHandling = NullValueHandling.Ignore)]
        public string Tilsynsist { get; set; }

        [JsonProperty("tilsynresultat", NullValueHandling = NullValueHandling.Ignore)]
        public string Tilsynresultat { get; set; }

        [JsonProperty("tilsynplanlagt", NullValueHandling = NullValueHandling.Ignore)]
        public string Tilsynplanlagt { get; set; }

        [JsonProperty("tilsyn", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Tilsyn> Tilsyn { get; set; }
    }

    public partial class Tilsyn
    {
        [JsonProperty("tilsynsdato", NullValueHandling = NullValueHandling.Ignore)]
        public string Tilsynsdato { get; set; }

        [JsonProperty("tilsynsstatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Tilsynsstatus { get; set; }
    }
}
