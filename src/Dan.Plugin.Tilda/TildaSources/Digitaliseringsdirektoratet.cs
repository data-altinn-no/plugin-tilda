using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Utils;
using Dan.Plugin.Tilda.Interfaces;
using Microsoft.Extensions.Logging;
using Nadobe.Common.Models;

namespace Dan.Plugin.Tilda.TildaSources
{
 

    public class Digitaliseringsdirektoratet : TildaDataSource, ITildaPdfReport//, ITildaAuditReports, ITildaNPDIDAuditReports, ITildaAuditCoordination, ITildaTrendReports, ITildaTrendReportsAll, ITildaAuditCoordinationAll, ITildaAuditReportsAll, ITildaAlertMessage
    {

        private const string orgNo = "991825827";
        public const string controlAgency = "Digitaliseringsdirektoratet";

        public override string ControlAgency => controlAgency;

        public override string OrganizationNumber => orgNo;

        public override bool TestOnly => true;

        public Digitaliseringsdirektoratet(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public Digitaliseringsdirektoratet() : base()
        {

        }

        public override async Task<TrendReportList> GetDataTrendAllAsync(EvidenceHarvesterRequest req, Int64? month, Int64? year, string filter)
        {
            return await GetDataTrendAsync(req, null, null);
        }

        public override async Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, Int64? month, Int64? year, string filter)
        {
            return await GetAuditReportsAsync(req, null, null);
        }
        public override async Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, Int64? month, Int64? year, string filter)
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


        public override async Task<AlertMessageList> GetAlertMessagesAsync(EvidenceHarvesterRequest req, int? month, int? year, string identifier)
        {
            var url = GetUriAll(BaseUri, AlertDatasetName, req.Requestor, month, year, identifier);
            var list = new AlertMessageList(OrganizationNumber);

            try
            {
                list.AlertMessages.AddRange(await new Mock().GetMockAlertMessages(req.OrganizationNumber, OrganizationNumber, ControlAgency, Guid.NewGuid().ToString()));

                if (list.AlertMessages.Count > 0)
                    list.SetStatusAndTextAndOwner("OK", Models.Enums.StatusEnum.OK, OrganizationNumber);
                else
                    list.SetStatusAndTextAndOwner("Tomt", Models.Enums.StatusEnum.NotFound, OrganizationNumber);
            } catch (Exception ex)
            {
                list.SetStatusAndTextAndOwner(ex.Message, Models.Enums.StatusEnum.Failed, OrganizationNumber);
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

            return await Helpers.GetPdfreport(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
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

        public override async Task<NPDIDAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string npdid)
        {
            var url = $"{BaseUri}/npdid/{req.OrganizationNumber}";

            var resultList = new NPDIDAuditReportList(OrganizationNumber);
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
    }

}
