using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Fiskeridirektoratet : TildaDataSource, ITildaAuditReports, ITildaAuditCoordination, ITildaTrendReports
    {
        private const string orgNo = "971203420";
        private const string controlAgency = "Fiskeridirektoratet";

        public override string OrganizationNumber => orgNo;

        public override string ControlAgency => controlAgency;

        public Fiskeridirektoratet(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public Fiskeridirektoratet()
        {

        }
    }
}
