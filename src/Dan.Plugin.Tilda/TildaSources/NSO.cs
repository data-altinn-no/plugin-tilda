using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class NSO : TildaDataSource
    {
        private const string orgNo = "960478150";
        public const string controlAgency = "Næringslivets sikkerhetsorganisasjon";

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public NSO(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {

        }

        public NSO() : base()
        {

        }
    }
}
