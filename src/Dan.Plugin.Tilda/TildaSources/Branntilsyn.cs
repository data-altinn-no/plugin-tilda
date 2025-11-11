using System;
using System.Globalization;
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
    public class Branntilsyn : TildaDataSource, ITildaAuditReports
    {
        private const string orgNo = "BRANN";
        public const string controlAgency = "BRANNTILSYN";
        //private const string url = "";
        //private List<string> urlList;

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;


        public Branntilsyn(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {

        }

        public Branntilsyn()
        {

        }

        /*
        public override async Task<AuditReportList> GetAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var urlList = BaseUri.Split(',');
            var result = new AuditReportList();


            foreach (var uri in this.urlList)
            {
                var url = GetUri(BaseUri, AuditReportDatasetName, req.OrganizationNumber, req.Requestor, fromDate, toDate);
                var tmp = (AuditReportList)await Helpers.GetData<AuditReportList>(url, OrganizationNumber, _client, _logger, req.MPToken);
                result.AuditReports.AddRange(tmp.AuditReports);
            }

            if (result.AuditReports != null && result.AuditReports.Count > 0)
                result.SetStatusAndTextAndOwner("OK", StatusEnum.OK, ControlAgency);
            else
                result.SetStatusAndTextAndOwner("Ingen data funnet", StatusEnum.NotFound, ControlAgency);

            return result;
        }
        */

        public override string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            string apiUrl = $"{baseUri}/{organizationNumber}?requestor={requestor}";

            if (fromDate != null)
            {
                apiUrl += $"&fromDate={((DateTime)fromDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.CurrentCulture)}";
            }

            if (toDate != null)
            {
                apiUrl += $"&toDate={((DateTime)toDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.CurrentCulture)}";
            }

            if (!string.IsNullOrEmpty(identifier))
            {
                apiUrl += $"&id={identifier}";
            }

            return apiUrl;
        }
    }
}
