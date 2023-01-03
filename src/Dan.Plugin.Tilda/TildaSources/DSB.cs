using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class DSB : TildaDataSource
    {
        private const string orgNo = "111111111";
        public const string controlAgency = "Direktoratet for sikkerhet og beredskap";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public DSB(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public DSB()
        {

        }
    }
}
