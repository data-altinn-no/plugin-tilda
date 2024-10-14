using System;
using System.Web;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models.AlertMessages;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Mappers;

public interface IAlertMessageMapper
{
    AlertMessage Map(AlertSourceMessage source, string sourceOrg);
}

public class AlertMessageMapper : IAlertMessageMapper
{
    private readonly Settings _settings;

    public AlertMessageMapper(IOptions<Settings> settings)
    {
        _settings = settings.Value;
    }

    public AlertMessage Map(AlertSourceMessage source, string sourceOrg)
    {
        var datasetUrl = GetDatasetUrl(source.MessageContent.MessageType, source.Subject, sourceOrg, source.Id);
        return new AlertMessage
        {
            Id = source.Id,
            Source = sourceOrg,
            Subject = source.Subject,
            Timestamp = source.Timestamp,
            MessageContent = source.MessageContent != null
                ? new AlertMessageContent
                {
                    MessageType = source.MessageContent.MessageType,
                    FreeText = source.MessageContent.FreeText,
                    DatasetUrl = datasetUrl
                }
                : null
        };
    }

    private string GetDatasetUrl(string messageType, string subject, string source, string id)
    {
        // TODO: use enum?
        var datasetType = messageType switch
        {
            "varsel-om-rapport" => "TildaTilsynsrapportv1",
            "varsel-om-koordinering" => "TildaTilsynskoordineringv1",
            _ => null
        };

        if (datasetType == null)
        {
            return null;
        }

        var path = $"/v1/directharvest/{datasetType}";
        var uriBuilder = new UriBuilder($"{_settings.DataAltinnNoBaseUrl}{path}")
        {
            Port = -1
        };
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["subject"] = subject;
        query["tilsynskilder"] = source;
        query["identifikator"] = id;
        query["envelope"] = false.ToString();
        uriBuilder.Query = query.ToString()!;
        return uriBuilder.ToString();
    }
}
