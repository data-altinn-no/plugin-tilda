using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources;

public class Dirmin(
    IOptions<Settings> settings,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    IUriFormatter uriFormatter)
    :   TildaDataSource(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter),
        ITildaAuditReports,
        ITildaAuditCoordination,
        ITildaAlertMessage
{
    public override string OrganizationNumber  => "974760282";
    public override string ControlAgency => "Direktoratet for mineralforvaltning med Bergmesteren for Svalbard";
}
