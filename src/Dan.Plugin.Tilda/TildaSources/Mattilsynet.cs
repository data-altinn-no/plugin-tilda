using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Mattilsynet : TildaDataSource//, ITildaAuditCoordination, ITildaAuditReports, ITildaTrendReports, ITildaAuditCoordinationAll
    {

        private const string orgNo = "985399077";
        private const string controlAgency = "Mattilsynet";

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public Mattilsynet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {

        }

        public Mattilsynet() : base()
        {

        }
    }
}


