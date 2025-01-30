using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CloudNative.CloudEvents;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.AlertMessages;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Services;

public interface IAlertMessageSender
{
    Task Send(string sourceOrganisation, AlertSourceMessage message);
}

public class AlertMessageSender : IAlertMessageSender
{
    private readonly ITildaSourceProvider _tildaSourceProvider;
    private readonly ILogger<AlertMessageSender> _logger;
    private readonly Settings _settings;

    private static readonly CloudEventAttribute ResourceAttribute =
        CloudEventAttribute.CreateExtension("resource", CloudEventAttributeType.String);

    public AlertMessageSender(
        ITildaSourceProvider tildaSourceProvider,
        ILogger<AlertMessageSender> logger,
        IOptions<Settings> settings)
    {
        _tildaSourceProvider = tildaSourceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task Send(string sourceOrganisation, AlertSourceMessage message)
    {
        var cloudEvent = ConvertToCloudEvent(sourceOrganisation, message);
        var tildaSource = _tildaSourceProvider.GetRelevantSources<ITildaAlertMessage>(message.Recipient).SingleOrDefault();
        if (tildaSource == null)
        {
            _logger.LogWarning("No recipient configured for alert message. Source: {source} Recipient: {recipient}", sourceOrganisation, message.Recipient);
            return;
        }
        await tildaSource.SendAlertMessageAsync(cloudEvent);
    }

    private CloudEvent ConvertToCloudEvent(string sourceOrganisation, AlertSourceMessage message)
    {
        var source = GetSourceUri(sourceOrganisation, message.Id);
        return new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = message.Id,
            Time = message.Timestamp,
            Type = PluginConstants.MtamCloudEventType,
            Source = source,
            Subject = sourceOrganisation,
            [ResourceAttribute] = PluginConstants.MtamResourceId
        };
    }

    private Uri GetSourceUri(string sourceOrganisation, string messageId)
    {
        const string path = "/v1/directharvest/TildaMeldingTilAnnenMyndighetv1";
        var uriBuilder = new UriBuilder($"{_settings.DataAltinnNoBaseUrl}{path}")
        {
            Port = -1
        };
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["tilsynskilder"] = sourceOrganisation;
        query["identifikator"] = messageId;
        query["envelope"] = false.ToString();
        uriBuilder.Query = query.ToString()!;
        return uriBuilder.Uri;
    }
}
