using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Justervesenet : TildaDataSource, ITildaAuditCoordination, ITildaAuditReports, ITildaTrendReports, ITildaAuditCoordinationAll
    {

        private const string orgNo = "874761192";
        private const string controlAgency = "Justervesenet";

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public Justervesenet(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public Justervesenet() : base()
        {

        }
    }
}


