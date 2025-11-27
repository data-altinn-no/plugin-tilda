using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Utils;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Tilda.Models.Audits.Alerts;
using Dan.Tilda.Models.Audits.Coordination;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Audits.Trend;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Digitaliseringsdirektoratet : TildaDataSource, ITildaAlertMessage, ITildaPdfReport//, ITildaAuditReports, ITildaNPDIDAuditReports, ITildaAuditCoordination, ITildaTrendReports, ITildaTrendReportsAll, ITildaAuditCoordinationAll, ITildaAuditReportsAll, ITildaAlertMessage
    {

        private const string orgNo = "991825827";
        public const string controlAgency = "Digitaliseringsdirektoratet";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public override bool TestOnly => true;

        private readonly string _code;

        public Digitaliseringsdirektoratet(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter)
        {
            _code = _settings.GetClassBaseCode(GetType().Name);
        }

        public Digitaliseringsdirektoratet() : base()
        {

        }

        public override async Task<TrendReportList> GetDataTrendAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {
            return await GetDataTrendAsync(req, null, null);
        }

        public override async Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {
            return await GetAuditReportsAsync(req, null, null);
        }
        public override async Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {

            var resultList = new AuditCoordinationList(OrganizationNumber);
            try
            {

                var mock = new Mock();

                resultList.AuditCoordinations.AddRange(await mock.GetMockCoordinationReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));
                resultList.AuditCoordinations.AddRange(await mock.GetMockCoordinationReports("983175155", OrganizationNumber, ControlAgency));
                resultList.AuditCoordinations.AddRange(await mock.GetMockCoordinationReports("999601391", OrganizationNumber, ControlAgency));

                if (resultList.AuditCoordinations.Count == 0)
                {
                    resultList = Helpers.GetEmptyResponseCoordinationList(OrganizationNumber);
                }
            }
            catch (Exception)
            {
                resultList = Helpers.GetEmptyFailedResponseCoordinationList(OrganizationNumber);
            }

            return resultList;


        }


        public override async Task<AlertMessageList> GetAlertMessagesAsync(EvidenceHarvesterRequest req, string month, string year, string identifier)
        {
            var url = GetUriAll(BaseUri, AlertDatasetName, req.Requestor, month, year, identifier);
            var list = new AlertMessageList(OrganizationNumber);

            try
            {
                list.AlertMessages.AddRange(await new Mock().GetMockAlertMessages(req.OrganizationNumber, OrganizationNumber, ControlAgency, Guid.NewGuid().ToString()));

                if (list.AlertMessages.Count > 0)
                    list.SetStatusAndTextAndOwner("OK", Dan.Tilda.Models.Enums.StatusEnum.Ok, OrganizationNumber);
                else
                    list.SetStatusAndTextAndOwner("Tomt", Dan.Tilda.Models.Enums.StatusEnum.NotFound, OrganizationNumber);
            } catch (Exception ex)
            {
                list.SetStatusAndTextAndOwner(ex.Message, Dan.Tilda.Models.Enums.StatusEnum.Failed, OrganizationNumber);
            }

            return list;
        }

        public override async Task<AuditReportList> GetAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var resultList = new AuditReportList(OrganizationNumber);
            try
            {

                var mock = new Mock();

                resultList.AuditReports.AddRange(await mock.GetMockAuditReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));
                resultList.AuditReports.AddRange(await mock.GetMockAuditReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));

                if (resultList.AuditReports.Count == 0)
                {
                    resultList = Helpers.GetEmptyResponseAuditReportList(OrganizationNumber);
                }
            }
            catch (Exception)
            {
                resultList = Helpers.GetEmptyFailedResponseAuditReportList(OrganizationNumber);
            }

            return resultList;
        }

        public override async Task<TrendReportList> GetDataTrendAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var resultList = new TrendReportList(OrganizationNumber);
            try
            {

                var mock = new Mock();

                resultList.TrendReports.AddRange(await mock.GetMockTrendReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));
                resultList.TrendReports.AddRange(await mock.GetMockTrendReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));

                if (resultList.TrendReports.Count == 0)
                {
                    resultList = Helpers.GetEmptyResponseTrendReportList(OrganizationNumber);
                }
            }
            catch (Exception)
            {
                resultList = Helpers.GetEmptyFailedResponseTrendReportList(OrganizationNumber);
            }

            return resultList;
        }

        public override async Task<byte[]> GetPdfReport(EvidenceHarvesterRequest req, string internTilsynsId)
        {
            var url = "https://www.orimi.com/pdf-test.pdf";

            return await _client.GetPdfreport(url, OrganizationNumber, _logger, req.MPToken, req.Requestor);
        }

        public override async Task<AuditCoordinationList> GetAuditCoordinationAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var resultList = new AuditCoordinationList(OrganizationNumber);
            try
            {

                var mock = new Mock();

                resultList.AuditCoordinations.AddRange(await mock.GetMockCoordinationReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));
                resultList.AuditCoordinations.AddRange(await mock.GetMockCoordinationReports(req.OrganizationNumber, OrganizationNumber, ControlAgency));

                if (resultList.AuditCoordinations.Count == 0)
                {
                    resultList = Helpers.GetEmptyResponseCoordinationList(OrganizationNumber);
                }
            }
            catch (Exception)
            {
                resultList = Helpers.GetEmptyFailedResponseCoordinationList(OrganizationNumber);
            }

            return resultList;
        }

        public override async Task<NpdidAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string npdid)
        {
            var url = $"{BaseUri}/npdid/{req.OrganizationNumber}";

            var resultList = new NpdidAuditReportList(OrganizationNumber);
            try
            {

                var mock = new Mock();

                resultList.AuditReports.AddRange(await mock.GetMockNPDIDAuditReports(req.OrganizationNumber, OrganizationNumber, ControlAgency, "342342"));

                if (resultList.AuditReports.Count == 0)
                {
                    resultList = Helpers.GetEmptyResponseNPDIDAuditReportList(OrganizationNumber);
                }
            }
            catch (Exception)
            {
                resultList = Helpers.GetEmptyFailedResponseNPDIDAuditReportList(OrganizationNumber);
            }

            return resultList;
        }

        protected override string GetAlertUri(string from) => $"{BaseUri}/{MtamDatasetName}?fromDate={from}&code={_code}";
        protected override string GetSingleAlertUri(string id, string requestor) =>
            $"{BaseUri}/{MtamDatasetName}/{id}?requestor={requestor}&code={_code}";

        protected override string PostAlertUri() => $"{BaseUri}/{MtamDatasetName}?code={_code}";
    }

}
