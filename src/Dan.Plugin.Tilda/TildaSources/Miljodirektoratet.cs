using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nadobe.Common.Models;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{

    public class Miljodirektoratet : TildaDataSource, ITildaAuditReports, ITildaAuditCoordination, ITildaTrendReports
    {
        public string orgNo = "999601391";
        public string controlAgency = "Miljødirektoratet";

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public Miljodirektoratet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider)
        {

        }

        public Miljodirektoratet() : base()
        {

        }
    }
}
