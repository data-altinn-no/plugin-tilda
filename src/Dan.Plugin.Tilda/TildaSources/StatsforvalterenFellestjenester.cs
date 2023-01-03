using System;
using System.Net.Http;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class StatsforvalterenFellestjenester : TildaDataSource
    {
        private const string orgNo = "921627009";
        public const string controlAgency = "Statsforvalterens fellestjenester";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public StatsforvalterenFellestjenester(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public StatsforvalterenFellestjenester()
        {

        }

        public static string GetSfUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            string apiUrl = $"{baseUri}/{dataset}/{organizationNumber}?requestor={requestor}";

            fromDate ??= DateTime.Now.AddYears(-1);
            toDate ??= DateTime.Now;

            apiUrl += $"&fromDate={((DateTime)fromDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";
            apiUrl += $"&toDate={((DateTime)toDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";

            return apiUrl;
        }

        public static string GetSfUriAll(string baseUri, string dataset, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            string apiUrl = $"{baseUri}/{dataset}/?requestor={requestor}";

            fromDate ??= DateTime.Now.AddYears(-1);
            toDate ??= DateTime.Now;

            apiUrl += $"&fromDate={((DateTime)fromDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";
            apiUrl += $"&toDate={((DateTime)toDate).ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.CurrentCulture)}";

            return apiUrl;
        }
    }
}
