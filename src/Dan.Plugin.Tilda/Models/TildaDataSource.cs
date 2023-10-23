using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nadobe.Common.Models;

namespace Dan.Plugin.Tilda.Models
{
    public abstract class TildaDataSource
    {
        public abstract string OrganizationNumber { get; }

        public abstract string ControlAgency { get; }

        public string BaseUri { get; set; }

        public virtual bool TestOnly => false;

        protected virtual string TrendDatasetName => "trend";

        protected virtual string CoordinationDatasetName => "koordinering";

        protected virtual string NpdidDatasetName => "npdid";

        protected virtual string AuditReportDatasetName => "tilsyn";

        protected virtual string AlertDatasetName => "meldingtilannenmyndighet";

        protected virtual string PdfReportDatasetName => "tilsyn/pdf";

        protected Settings _settings;
        protected ILogger _logger;
        protected HttpClient _client;

        public TildaDataSource(Settings settings, HttpClient client, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
            _client = client;
            BaseUri = _settings.GetClassBaseUri(GetType().Name);
        }

        public TildaDataSource()
        {
        }

        public virtual async Task<AuditReportList> GetAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var url = GetUri(BaseUri, AuditReportDatasetName, req.OrganizationNumber, req.Requestor, fromDate,
                toDate);
            return await Helpers.GetData<AuditReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {
            var url = GetUriAll(BaseUri, AuditReportDatasetName, req.Requestor, month, year, string.Empty, string.Empty, filter);
            return await Helpers.GetData<AuditReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<TrendReportList> GetDataTrendAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var url = GetUri(BaseUri, TrendDatasetName, req.OrganizationNumber, req.Requestor, fromDate, toDate);
            return await Helpers.GetData<TrendReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<TrendReportList> GetDataTrendAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {
            var url = GetUriAll(BaseUri, TrendDatasetName, req.Requestor, month, year, string.Empty, string.Empty, filter);
            return await Helpers.GetData<TrendReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<AuditCoordinationList> GetAuditCoordinationAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate)
        {
            var url = GetUri(BaseUri, CoordinationDatasetName, req.OrganizationNumber, req.Requestor, fromDate, toDate);
            return await Helpers.GetData<AuditCoordinationList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter)
        {
            var url = GetUriAll(BaseUri, CoordinationDatasetName, req.Requestor, month, year, string.Empty, string.Empty, filter);
            return await Helpers.GetData<AuditCoordinationList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<NPDIDAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string npdid)
        {
            var url = GetUri(BaseUri, NpdidDatasetName, req.OrganizationNumber, req.Requestor, fromDate, toDate, null, npdid);
            return await Helpers.GetData<NPDIDAuditReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<NPDIDAuditReportList> GetNPDIDAuditReportsAllAsync(EvidenceHarvesterRequest req, string month, string year, string npdid, string filter)
        {
            var url = GetUriAll(BaseUri, NpdidDatasetName, req.Requestor, month, year, null, npdid);
            return await Helpers.GetData<NPDIDAuditReportList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual async Task<AlertMessageList> GetAlertMessagesAsync(EvidenceHarvesterRequest req, string month, string year, string identifier)
        {
            var url = GetUriAll(BaseUri, AlertDatasetName, req.Requestor, month, year, identifier, null);
            return await Helpers.GetData<AlertMessageList>(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            return Helpers.GetUri(baseUri, dataset, organizationNumber, requestor, fromDate, toDate, null, npdid);
        }

        public virtual async Task<byte[]> GetPdfReport(EvidenceHarvesterRequest req, string internTilsynsId)
        {
            var url = GetUri(BaseUri, PdfReportDatasetName, req.Requestor, null, null, null, internTilsynsId);
            return await Helpers.GetPdfreport(url, OrganizationNumber, _client, _logger, req.MPToken, req.Requestor);
        }

        public virtual string GetUriAll(string baseUri, string dataset, string requestor, string month, string year, string identifier = "", string npdid = "", string filter = "")
        {
            return Helpers.GetUriAll(baseUri, dataset, requestor, month, year, identifier, null, filter);
        }
    }
}
