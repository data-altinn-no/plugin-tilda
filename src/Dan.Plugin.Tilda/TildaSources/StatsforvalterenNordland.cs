using System;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class StatsforvalterenNordland : TildaDataSource, ITildaAuditCoordination, ITildaAuditCoordinationAll, ITildaAuditReports
    {
        private const string orgNo = "974764687";
        public const string controlAgency = "Statsforvalteren i Nordland";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public StatsforvalterenNordland(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider)
        {

        }

        public StatsforvalterenNordland()
        {

        }

        public override string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            return StatsforvalterenFellestjenester.GetSfUri(baseUri, dataset, organizationNumber, requestor, fromDate,
                toDate, identifier, npdid);
        }

        public override string GetUriAll(string baseUri, string dataset, string requestor, string month, string year, string identifier = "", string npdid = "", string filter = "")
        {
            return StatsforvalterenFellestjenester.GetSfUriAll(baseUri, dataset, requestor, month, year, identifier, npdid, filter);
        }
    }
}
