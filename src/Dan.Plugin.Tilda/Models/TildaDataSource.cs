using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.NewtonsoftJson;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.AlertMessages;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly.Registry;

namespace Dan.Plugin.Tilda.Models
{
    public abstract class TildaDataSource : ITildaDataSource
    {
        private readonly ResiliencePipelineProvider<string> _pipelineProvider;
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

        protected virtual string MtamDatasetName => "mtam";

        protected Settings _settings;
        protected ILogger _logger;
        protected HttpClient _client;
        protected HttpClient _alertClient;

        public TildaDataSource(
            IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider)
        {
            _pipelineProvider = pipelineProvider;
            _settings = settings.Value;
            _logger = loggerFactory.CreateLogger<TildaDataSource>();
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _alertClient = httpClientFactory.CreateClient("AlertHttpClient");
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

        // TODO: Shouldn't return empty list on error. Need to ensure that we don't break the flow, but also don't update
        // counter on failures
        // TODO: Rename to alert
        public virtual async Task<List<AlertSourceMessage>> GetAlertMessagesAsync(string from)
        {
            var targetUrl = $"{BaseUri}/{MtamDatasetName}?fraDato={from}";
            string responseString;
            try
            {
                var response = await _client.GetAsync(targetUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to get mtam messages from org={organizationNumber} on url={targetUrl} with unsuccessful response status={statusCode}",
                        OrganizationNumber, targetUrl, response.StatusCode
                    );
                    return new List<AlertSourceMessage>();
                }
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to get mtam messages from org={organizationNumber} on url={targetUrl} with exception ex={ex} message={message} status={status}",
                    OrganizationNumber, targetUrl, ex.GetType().Name, ex.Message, "hardfail"
                );
                return new List<AlertSourceMessage>();
            }

            try
            {
                var mtamMessages = JsonConvert.DeserializeObject<List<AlertSourceMessage>>(responseString);
                return mtamMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to deserialize mtam messages from org={organizationNumber} with exception ex={ex} message={message} status={status}",
                    OrganizationNumber, ex.GetType().Name, ex.Message, "hardfail"
                );
                return new List<AlertSourceMessage>();
            }
        }

        public virtual async Task<AlertSourceMessage> GetAlertMessageAsync(EvidenceHarvesterRequest req,
            string identifier)
        {
            var targetUrl = $"{BaseUri}/{MtamDatasetName}/{identifier}";
            string responseString;
            try
            {
                var response = await _client.GetAsync(targetUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to get mtam messages from org={organizationNumber} on url={targetUrl} with unsuccessful response status={statusCode}",
                        OrganizationNumber, targetUrl, response.StatusCode
                    );
                    // TODO: consider if null is appropriate?
                    return null;
                }
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to get mtam messages from org={organizationNumber} on url={targetUrl} with exception ex={ex} message={message} status={status}",
                    OrganizationNumber, targetUrl, ex.GetType().Name, ex.Message, "hardfail"
                );
                return null;
            }

            try
            {
                var mtamMessage = JsonConvert.DeserializeObject<AlertSourceMessage>(responseString);
                return mtamMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to deserialize mtam messages from org={organizationNumber} with exception ex={ex} message={message} status={status}",
                    OrganizationNumber, ex.GetType().Name, ex.Message, "hardfail"
                );
                return null;
            }
        }

        public async Task SendAlertMessageAsync(CloudEvent cloudEvent)
        {
            var targetUrl = $"{BaseUri}/{MtamDatasetName}";
            var resiliencePipeline = _pipelineProvider.GetPipeline("alert-pipeline");
            CloudEventFormatter formatter = new JsonEventFormatter();
            var cloudEventContent = cloudEvent.ToHttpContent(ContentMode.Structured, formatter);
            HttpResponseMessage response;
            await resiliencePipeline.ExecuteAsync(async token =>
            {
                response = await _alertClient.PostAsync(targetUrl, cloudEventContent, token);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unable to post alert message to {targetUrl}, code {response.StatusCode}, reason: {response.ReasonPhrase}");
                }
            });
        }
    }
}
