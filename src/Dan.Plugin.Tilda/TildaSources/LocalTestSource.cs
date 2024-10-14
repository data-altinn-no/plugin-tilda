using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources;

public class LocalTestSource : TildaDataSource, ITildaAlertMessage
{
    public override string OrganizationNumber => "1111111111";
    public override string ControlAgency => "Local";
    public override bool TestOnly => true;

    public LocalTestSource(
        IOptions<Settings> settings,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        ResiliencePipelineProvider<string> pipelineProvider) :
        base(settings, httpClientFactory, loggerFactory, pipelineProvider)
    {

    }
}
