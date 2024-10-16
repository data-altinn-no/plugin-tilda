using System;
using System.Globalization;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Eltilsyn : TildaDataSource, ITildaAuditReports
    {
        private const string orgNo = "DLE";
        public const string controlAgency = "ELTILSYN";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public Eltilsyn(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider)
        {

        }

        public Eltilsyn()
        {

        }

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
