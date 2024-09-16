using System;
using System.Net.Http;
using System.Web;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class StatsforvalterenFellestjenester : TildaDataSource
    {
        private const string orgNo = "921627009";
        public const string controlAgency = "Statsforvalterens fellestjenester";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public StatsforvalterenFellestjenester(IOptions<Settings> settings, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) :
            base(settings, httpClientFactory, loggerFactory)
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

        public static string GetSfUriAll(string baseUri, string dataset, string requestor, string month, string year, string identifier = "", string npdid = "", string filter="")
        {
            string apiUrl = $"{baseUri}/{dataset}/?requestor={requestor}";

            if (!string.IsNullOrEmpty(month))
                apiUrl += $"&maaned={month}";

            if (!string.IsNullOrEmpty(year))
                apiUrl += $"&aar={year}";

            if (!string.IsNullOrEmpty(filter))
                apiUrl += $"&filter={HttpUtility.UrlEncode(filter)}";

            return apiUrl;
        }
    }
}
