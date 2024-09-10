using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models;

[Serializable]
public class KofuviResponse
{
    [JsonProperty("_embedded")]
    public Embedded Embedded { get; set; }
}

[Serializable]
public class Embedded
{
    [JsonProperty("varsling")]
    public Notification Notification { get; set; }
}

[Serializable]
public class Notification
{
    [JsonProperty("varslingsadresser")]
    public List<NotificationAddress> NotificationAddresses { get; set; }
}

[Serializable]
public class NotificationAddress
{
    [JsonProperty("kontaktinformasjon")]
    public ContactInformation ContactInformation { get; set; }
}

[Serializable]
public class ContactInformation
{
    [JsonProperty("digitalVarslingsinformasjon")]
    public DigitalNotificationInformation DigitalNotificationInformation { get; set; }
}

[Serializable]
public class DigitalNotificationInformation
{
    [JsonProperty("epostadresse")]
    public NotificationEmail NotificationEmail { get; set; }
}

[Serializable]
public class NotificationEmail
{
    [JsonProperty("fullstendigAdresse")]
    public string CompleteEmail { get; set; }
}
