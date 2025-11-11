using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{


    public class Helsetilsynet : TildaDataSource
    {

        private const string orgNo = "974761394";
        public const string controlAgency = "Helsetilsynet";

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public override string OrganizationNumber
        {
            get => orgNo;
        }


        public Helsetilsynet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {

        }

        public override string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            string apiUrl = $"{baseUri}/{dataset}/v1/{organizationNumber}/{requestor}";

            fromDate = fromDate ?? DateTime.Now.AddYears(-1);
            toDate = toDate ?? DateTime.Now;

            apiUrl += $"/{((DateTime)fromDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";
            apiUrl += $"/{((DateTime)toDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";

            return apiUrl;
        }

        public Helsetilsynet() : base()
        {

        }
    }

}
