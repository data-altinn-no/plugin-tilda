using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models.Cosmos;

[Serializable]
public class MtamCounter
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("lastFetched")]
    public DateTime LastFetched { get; set; }

    [JsonProperty(NullValueHandling=NullValueHandling.Ignore, PropertyName="_etag")]
    public string Etag { get; set; }
}
