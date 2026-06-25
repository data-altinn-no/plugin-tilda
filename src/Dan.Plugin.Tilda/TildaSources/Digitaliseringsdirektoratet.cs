using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Utils;
using Dan.Plugin.Tilda.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Digitaliseringsdirektoratet : TildaDataSource //ITildaAlertMessage, ITildaPdfReport, ITildaNPDIDAuditReports//, ITildaAuditReports, ITildaAuditCoordination, ITildaTrendReports, ITildaTrendReportsAll, ITildaAuditCoordinationAll, ITildaAuditReportsAll, ITildaAlertMessage
    {

        private const string orgNo = "991825827";
        public const string controlAgency = "Digitaliseringsdirektoratet";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public override bool TestOnly => true;

        public Digitaliseringsdirektoratet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter,
            IMaskinportenService maskinportenService) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter, maskinportenService)
        {
        }

        public Digitaliseringsdirektoratet() : base()
        {

        }
    }

}
