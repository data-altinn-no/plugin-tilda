using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Options;
using Nadobe.Common.Models;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Arbeidstilsynet : TildaDataSource, ITildaAuditReports, ITildaAuditCoordination, ITildaTrendReports, ITildaAuditCoordinationAll, ITildaAuditReportsAll
    {
        private const string orgNo = "974761211";
        private const string controlAgency = "Arbeidstilsynet";

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public Arbeidstilsynet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider)
        {
        }


        public Arbeidstilsynet() : base()
        {
        }
    }
}
