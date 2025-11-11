using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Fiskeridirektoratet : TildaDataSource, ITildaAuditReports, ITildaAuditCoordination, ITildaTrendReports
    {
        private const string orgNo = "971203420";
        private const string controlAgency = "Fiskeridirektoratet";

        public override string OrganizationNumber => orgNo;

        public override string ControlAgency => controlAgency;

        public Fiskeridirektoratet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {

        }

        public Fiskeridirektoratet()
        {

        }
    }
}
