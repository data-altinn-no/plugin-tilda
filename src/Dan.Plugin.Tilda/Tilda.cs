using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Models.Enums;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Dan.Plugin.Tilda
{
    public class Tilda
    {
        private readonly IPolicyRegistry<string> _policyRegistry;
        private ILogger _logger;
        private HttpClient _client;
        private HttpClient _erClient;
        private HttpClient _kofuviClient;
        private Settings _settings;
        private readonly IEntityRegistryService _entityRegistryService;
        private readonly IEvidenceSourceMetadata _metadata;
        private List<string> P6Orgs;
        private List<string> P9Orgs;

        public Tilda(IHttpClientFactory httpClientFactory, IOptions<Settings> settings, IPolicyRegistry<string> policyRegistry, IEntityRegistryService entityRegistry, IEvidenceSourceMetadata metadata)
        {
            _policyRegistry = policyRegistry;
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _erClient = httpClientFactory.CreateClient("ERHttpClient");
            _kofuviClient = httpClientFactory.CreateClient("KofuviClient");
            _settings = settings.Value;
            _entityRegistryService = entityRegistry;
            _entityRegistryService.AllowTestCcrLookup = _settings.IsLocalDevelopment || _settings.IsLocalDevelopment;
            _metadata = metadata;
        }

        [Function("TildaMeldingTilAnnenMyndighetv1")]
        public async Task<HttpResponseData> TildaMeldingTilAnnenMyndighet([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaMeldingTilAnnenMyndighetv1")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesMeldingTilAnnenMyndighet(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaStorulykkevirksomhet")]
        public async Task<HttpResponseData> TildaStorulykkevirksomhet([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaStorulykkevirksomhet")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            await GetStorulykkeProps();

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesStorulykkevirksomhet(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }


        [Function("TildaStorulykkevirksomhetAlle")]
        public async Task<HttpResponseData> TildaStorulykkevirksomhetAlle([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaStorulykkevirksomhetAlle")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            await GetStorulykkeProps();

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesStorulykkevirksomhetAlle(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaTilsynsrapportv1")]
        public async Task<HttpResponseData> Tilsynsrapport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportv1")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynsrapport(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaTilsynsrapportAllev1")]
        public async Task<HttpResponseData> TilsynsrapportAlle(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportAllev1")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynsRapportAllAsync(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaTilsynskoordineringv1")]
        public async Task<HttpResponseData> Tilsynskoordinering(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynskoordineringv1")] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynskoordinering(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaTilsynskoordineringAllev1")]
        public async Task<HttpResponseData> TilsynskoordineringAlle(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynskoordineringAllev1")] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynskoordingeringAllASync(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }


        [Function("TildaTrendrapportv1")]
        public async Task<HttpResponseData> Trend(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTrendrapportv1")] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTrend(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaTrendrapportAllev1")]
        public async Task<HttpResponseData> TrendAlle(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTrendrapportAllev1")] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTrendAll(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaNPDIDv1")]
        public async Task<HttpResponseData> NPDID(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaNPDIDv1")] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesNpdid(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
        }

        [Function("TildaMetadatav1")]
        public async Task<HttpResponseData> TildaMetadata(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaMetadatav1")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTildaMetadata(evidenceHarvesterRequest));
        }

        [Function("TildaTilsynsrapportpdfv1")]
        public async Task<HttpResponseData> TildaPdfReport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportpdfv1")] HttpRequestData req, FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTildaPdfReportV1(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTildaPdfReportV1(EvidenceHarvesterRequest req)
        {
            var taskList = new List<Task<string>>();

            if (!req.TryGetParameter("internTilsynsId", out string id))
            {
                throw new EvidenceSourcePermanentClientException(1, $"Missing required parameter internTilsynsId");
            }

            string filter = req.SubjectParty?.NorwegianOrganizationNumber;
            byte[] result;

            try
            {;
                //Should always only return ONE source
                var pdfTarget = SourcesHelper.GetRelevantSources<ITildaPdfReport>(filter, _client, _logger, _settings).FirstOrDefault();

                if (pdfTarget == null)
                {
                    _logger.LogError($"plugin tilda does not support pdf for source {filter}");
                    throw new EvidenceSourcePermanentClientException(1, $"Source {filter} does not support pdf reports");
                }

                result = await pdfTarget.GetPdfReport(req, id);
                var ecb = new EvidenceBuilder(_metadata, "TildaTilsynsrapportpdfv1");
                ecb.AddEvidenceValue($"pdfrapport", result, "Tilda", false);

                return ecb.GetEvidenceValues();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new EvidenceSourcePermanentClientException(1, $"Source {filter} does not support pdf reports");
            }
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesMeldingTilAnnenMyndighet(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var taskList = new List<Task<AlertMessageList>>();
            try
            {
                foreach (ITildaAlertMessage a in SourcesHelper.GetRelevantSources<ITildaAlertMessage>(param.sourceFilter, _client, _logger, _settings))
                {
                    taskList.Add(a.GetAlertMessagesAsync(req, param.fromDate, param.toDate, param.identifier));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new EvidenceSourcePermanentClientException(1, $"Could not create requests for specified sources ({ex.Message}");
            }

            await Task.WhenAll(taskList);
            var list = new List<IAuditList>();

            foreach (var task in taskList)
            {
                var values = task.Result;

                if (values.Status == StatusEnum.NotFound || values.Status == StatusEnum.Failed || values.Status == StatusEnum.Unknown)
                {
                    values.AlertMessages = null;
                }

                list.Add(values);
            }

            return BuildEvidenceValueList("TildaMeldingTilAnnenMyndighetv1", "meldingTilAnnenMyndighet", list);
        }


        private List<EvidenceValue> BuildEvidenceValueList(string evidenceCodeName, string evidenceValueName, List<IAuditList> input)
        {

            var ecb = new EvidenceBuilder(_metadata, evidenceCodeName);

            foreach (var b in input)
            {
                ecb.AddEvidenceValue(evidenceValueName, JsonConvert.SerializeObject(b, Formatting.None), b.GetOwner(), false);
            }

            return ecb.GetEvidenceValues();
        }


        private async Task<List<EvidenceValue>> GetEvidenceValuesTildaMetadata(EvidenceHarvesterRequest req)
        {
            var ecb = new EvidenceBuilder(_metadata, "TildaMetadatav1");

            var trend = SourcesHelper.GetAllSources<ITildaTrendReports>(_settings, _client, _logger, false).Select(x=>x.OrganizationNumber + ":" + x.ControlAgency);
            var trendAll = SourcesHelper.GetAllSources<ITildaTrendReportsAll>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var audit = SourcesHelper.GetAllSources<ITildaAuditReports>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
            var auditAll = SourcesHelper.GetAllSources<ITildaAuditReportsAll>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var coordination = SourcesHelper.GetAllSources<ITildaAuditCoordination>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
            var coordinationAll = SourcesHelper.GetAllSources<ITildaAuditCoordinationAll>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var npdid = SourcesHelper.GetAllSources<ITildaNPDIDAuditReports>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var pdfReport = SourcesHelper.GetAllSources<ITildaPdfReport>(_settings, _client, _logger, false).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var all = SourcesHelper.GetAllRegisteredSources(_settings).Select(x => x.OrganizationNumber + ":" + x.ControlAgency);



            ecb.AddEvidenceValue("TildaTrendrapportv1", string.Join(",", trend), "Tilda", false);
            ecb.AddEvidenceValue("TildaTrendrapportAllev1", string.Join(",", trendAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynsrapportv1", string.Join(",", audit), "Tilda", false);
            ecb.AddEvidenceValue("TildaTilsynsrapportAllev1", string.Join(",", auditAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynskoordineringv1", string.Join(",", coordination), "Tilda", false);
            ecb.AddEvidenceValue("TildaTilsynskoordineringAllev1", string.Join(",", coordinationAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaNPDIDv1", string.Join(",", npdid), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynsrapportpdfv1", string.Join(",", pdfReport), "Tilda", false);

            ecb.AddEvidenceValue("AlleKilder", string.Join(",", all), "Tilda", false);

            return await Task.FromResult(ecb.GetEvidenceValues());
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesNpdid(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var brResultTask = GetOrganizationsFromBR(req.OrganizationNumber, param);

            var taskList = new List<Task<NPDIDAuditReportList>>();
            try
            {
                foreach (ITildaNPDIDAuditReports a in SourcesHelper.GetRelevantSources<ITildaNPDIDAuditReports>(param.sourceFilter, _client, _logger, _settings))
                {
                    taskList.Add(a.GetNPDIDAuditReportsAsync(req, param.fromDate, param.toDate, param.npdid));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new EvidenceSourcePermanentClientException(1, $"Could not create requests for specified sources ({ex.Message}");
            }

            await Task.WhenAll(taskList);
            var brResult = await brResultTask;
            var list = new List<NPDIDAuditReportList>();

            foreach (var task in taskList)
            {
                var values = task.Result;

                if (values.Status == StatusEnum.NotFound || values.Status == StatusEnum.Failed || values.Status == StatusEnum.Unknown)
                {
                    values.AuditReports = null;
                }

                list.Add(values);
            }

            var ecb = new EvidenceBuilder(_metadata, "TildaNPDIDv1");

            foreach (var unit in brResult)
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
            try
            {
                foreach (var a in list)
                {
                    var filtered = (NPDIDAuditReportList)Helpers.Filter(a, brResult);
                    ecb.AddEvidenceValue($"tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return ecb.GetEvidenceValues();
        }

        private TildaParameters GetValuesFromParameters(EvidenceHarvesterRequest req)
        {
            DateTime? fromDateTime = null;
            DateTime? toDateTime = null;

            if (req.TryGetParameter("startdato", out DateTime fromDate))
            {
                fromDateTime = fromDate.ToUniversalTime();
            }

            if (req.TryGetParameter("sluttdato", out DateTime toDate))
            {
                toDateTime = toDate.ToUniversalTime();
            }

            req.TryGetParameter("npdid", out string npdid);
            req.TryGetParameter("tilsynskilder", out string sourceFilter);
            req.TryGetParameter("inkluderUnderenheter", out bool includeSubunits);
            req.TryGetParameter("identifikator", out string identifier);
            req.TryGetParameter("filter", out string filter);
            req.TryGetParameter("aar", out string year);
            req.TryGetParameter("maaned", out string month);

            /*
            var fromDate = req.GetOptionalParameterValue<DateTime?>("startdato")?.ToUniversalTime();
            var toDate = req.GetOptionalParameterValue<DateTime?>("sluttdato")?.ToUniversalTime();
            var npdid = req.GetOptionalParameterValue<string>("npdid");
            var sourceFilter = req.GetOptionalParameterValue<string>("tilsynskilder");
            var includeSubunits = req.GetOptionalParameterValue<bool?>("inkluderUnderenheter");
            var identifier = req.GetOptionalParameterValue<string>("identifikator");
            var filter = req.GetOptionalParameterValue<string>("filter");
            var year = req.GetOptionalParameterValue<Int64?>("aar");
            var month = req.GetOptionalParameterValue<Int64?>("maaned");
            */

            if (includeSubunits)
                throw new Exception("inkluderUnderenheter er ikke støttet ennå :)");

            return new TildaParameters(fromDateTime, toDateTime, npdid, false, sourceFilter, identifier, filter, year, month);
        }


        private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhetAlle(EvidenceHarvesterRequest evidenceHarvesterRequest, TildaParameters tildaParameters)
        {
            var eb = new EvidenceBuilder(_metadata, "TildaStorulykkevirksomhetAlle");
            eb.AddEvidenceValue("StorulykkevirksomheterParagraf6", JsonConvert.SerializeObject(new StorulykkevirksomhetListe() { Organizations = P6Orgs }), "Tilda", false);
            eb.AddEvidenceValue("StorulykkevirksomheterParagraf9", JsonConvert.SerializeObject(new StorulykkevirksomhetListe() { Organizations = P9Orgs }), "Tilda", false);
            return eb.GetEvidenceValues();
        }

        private async Task GetStorulykkeProps()
        {
            P6Orgs = await Helpers.GetParagraph("6");
            P9Orgs = await Helpers.GetParagraph("9");
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhet(EvidenceHarvesterRequest evidenceHarvesterRequest, TildaParameters tildaParameters)
        {
            var eb = new EvidenceBuilder(_metadata, "TildaStorulykkevirksomhet");

            var result = new StorulykkevirksomhetKontroll();
            result.OrganizationNumber = evidenceHarvesterRequest.OrganizationNumber;

            if (P6Orgs.Contains(evidenceHarvesterRequest.OrganizationNumber))
            {
                result.Paragraph6 = true;
            }

            if (P9Orgs.Contains(evidenceHarvesterRequest.OrganizationNumber))
            {
                result.Paragraph9 = true;

            }

            eb.AddEvidenceValue("Storulykkevirksomhet", JsonConvert.SerializeObject(result), "Tilda", false);

            return eb.GetEvidenceValues();
        }

        // TODO: use IncludeSubunits-param in Tildaparameters? And then we can reduce duplicate code between the overloads
        private async Task<List<TildaRegistryEntry>> GetOrganizationsFromBR(string organizationNumber, TildaParameters param)
        {
            var result = new List<TildaRegistryEntry>();
            var brResult = await Helpers.GetFromBR(organizationNumber, _erClient, false, _policyRegistry);
            AccountsInformation accountsInformation = null;
            if (string.IsNullOrEmpty(brResult.First().OverordnetEnhet))
            {
                accountsInformation = await Helpers.GetAnnualTurnoverFromBR(organizationNumber, _client, _policyRegistry);
            }

            var kofuviAddresses = await Helpers.GetKofuviAddresses(_settings.KofuviEndpoint, organizationNumber, _kofuviClient, _logger);

            var organization = await ConvertBRtoTilda(brResult.First(), accountsInformation);
            if (kofuviAddresses.Count > 0)
            {
                organization.Emails = kofuviAddresses;
            }
            result.Add(organization);

            return result;
        }

        private async Task<TildaRegistryEntry> GetOrganizationFromBR(string organizationNumber)
        {
            var brResultTask = await Helpers.GetFromBR(organizationNumber, _erClient, false, _policyRegistry);
            var accountsInformationTask = await Helpers.GetAnnualTurnoverFromBR(organizationNumber, _client, _policyRegistry);

            var kofuviAddresses = await Helpers.GetKofuviAddresses(_settings.KofuviEndpoint, organizationNumber, _kofuviClient, _logger);

            var organization = await ConvertBRtoTilda(brResultTask.First(), accountsInformationTask);
            if (kofuviAddresses.Count > 0)
            {
                organization.Emails = kofuviAddresses;
            }

            return organization;
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordinering(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var brResultTask = GetOrganizationsFromBR(req.OrganizationNumber, param);

            var taskList = new List<Task<AuditCoordinationList>>();
            try
            {
                foreach (ITildaAuditCoordination a in SourcesHelper.GetRelevantSources<ITildaAuditCoordination>(param.sourceFilter, _client, _logger, _settings))
                {
                    taskList.Add(a.GetAuditCoordinationAsync(req, param.fromDate, param.toDate));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            await Task.WhenAll(taskList);
            var brResult = await brResultTask;
            var list = new List<AuditCoordinationList>();

            foreach (var task in taskList)
            {
                var values = task.Result;

                if (values.Status == StatusEnum.NotFound || values.Status == StatusEnum.Failed || values.Status == StatusEnum.Unknown)
                {
                    values.AuditCoordinations = null;
                }

                list.Add(values);
            }

            var ecb = new EvidenceBuilder(_metadata, "TildaTilsynskoordineringv1");
            foreach (var unit in brResult)
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

            foreach (var a in list)
            {
                var filtered = (AuditCoordinationList)Helpers.Filter(a, brResult);
                ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), filtered.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }

        private async Task<TildaRegistryEntry> ConvertBRtoTilda(BREntityRegisterEntry brResult, AccountsInformation accountsInformation)
        {
            // *** TEMPORARY TILDA FILTERING ***
            if (brResult.Organisasjonsform == null)
            {
                return new TildaRegistryEntry()
                {
                    OrganisationForm = "",
                    OperationalStatus = OperationStatus.Blank,
                    OrganizationNumber = brResult.Organisasjonsnummer.ToString()
                };
            }

            if (brResult.Organisasjonsform.Kode == "TEST")
            {
                return new TildaRegistryEntry()
                {
                    OrganisationForm = brResult.Organisasjonsform.Kode,
                    OperationalStatus = GetOperationStatus(brResult),
                    OrganizationNumber = brResult.Organisasjonsnummer.ToString()
                };
            }

            //remove financial information from ENKs - GDPR
            if (brResult.Organisasjonsform.Kode == "ENK")
            {
                accountsInformation = null;
            }

            var item =  new TildaRegistryEntry()
            {
                OrganisationForm = brResult.Organisasjonsform.Kode,
                Name = brResult.Navn,
                Accounts = accountsInformation,
                BusinessCode = brResult.Naeringskode1?.Kode,
                OperationalStatus = GetOperationStatus(brResult),
                OrganizationNumber = brResult.Organisasjonsnummer.ToString()
            };


            if (brResult.Forretningsadresse != null)
            {
                item.PublicLocationAddress = new ERAddress()
                {
                    AddressName = string.Join(",", brResult.Forretningsadresse?.Adressenavn),
                    PostNumber = brResult.Forretningsadresse?.Postnummer,
                    PostName = brResult.Forretningsadresse?.Poststed,
                    CountyNumber = brResult.Forretningsadresse?.Kommunenummer,
                    MunicipalityNumber = brResult.Forretningsadresse?.Kommunenummer
                };
            }
            return item;
        }

        private OperationStatus GetOperationStatus(BREntityRegisterEntry brResult)
        {
            if (brResult.Konkurs)
            {
                return OperationStatus.Konkurs;
            }

            if (brResult.UnderAvvikling)
            {
                return OperationStatus.UnderAvvikling;
            }

            if (brResult.UnderTvangsavviklingEllerTvangsopplosning)
            {
                return OperationStatus.UnderTvangsavviklingEllerTvangsopplosning;
            }

            if (brResult.Slettedato != DateTime.MinValue)
            {
                return OperationStatus.Slettet;
            }

            return OperationStatus.OK;
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTrend(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var brResultTask = GetOrganizationsFromBR(req.OrganizationNumber, param);

            var taskList = new List<Task<TrendReportList>>();
            try
            {
                foreach (ITildaTrendReports a in SourcesHelper.GetRelevantSources<ITildaTrendReports>(param.sourceFilter, _client, _logger, _settings))
                {
                    taskList.Add( a.GetDataTrendAsync(req, param.fromDate, param.toDate));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            await Task.WhenAll(taskList);
            var brResult = await brResultTask;
            var list = new List<TrendReportList>();

            foreach (var task in taskList)
            {
                var values = task.Result;

                if (values.Status == StatusEnum.NotFound || values.Status == StatusEnum.Failed || values.Status == StatusEnum.Unknown)
                {
                    values.TrendReports = null;
                }

                list.Add(values);
            }

            var ecb = new EvidenceBuilder(_metadata, "TildaTrendrapportv1");

            foreach (var unit in brResult)
            {
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
            }

            foreach (var a in list)
            {
                var filtered = (TrendReportList)Helpers.Filter(a, brResult);
                ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
            }


            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTrendAll(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = SourcesHelper.GetRelevantSources<ITildaTrendReportsAll>(req.OrganizationNumber, _client, _logger, _settings);
            TrendReportList result = null;
            var brResults = new List<TildaRegistryEntry>();
            var ecb = new EvidenceBuilder(_metadata, "TildaTrendrapportAllev1");


            if (sourceList.Count()!= 1) //should only return the one source
                throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");

            try
            {
                result = await sourceList?.First().GetDataTrendAllAsync(req, param.month, param.year, param.filter);

                if (result.Status == StatusEnum.NotFound || result.Status == StatusEnum.Failed || result.Status == StatusEnum.Unknown)
                {
                    result.TrendReports = null;
                }

                var taskList = new List<Task<TildaRegistryEntry>>();

                if (result.TrendReports != null)
                {
                    var distinctList = result.TrendReports.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
                    foreach (var item in distinctList)
                    {
                        taskList.Add(GetOrganizationFromBR(item.ControlObject));
                    }
                }

                await Task.WhenAll(taskList);

                foreach (var t in taskList)
                {
                    brResults.Add(t.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            foreach (var unit in brResults)
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

            if (result != null)
            {
                var filtered = (TrendReportList)Helpers.Filter(result, brResults);
                ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordingeringAllASync(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = SourcesHelper.GetRelevantSources<ITildaAuditCoordinationAll>(req.OrganizationNumber, _client, _logger, _settings);
            AuditCoordinationList result = null;
            var brResults = new List<TildaRegistryEntry>();
            var ecb = new EvidenceBuilder(_metadata, "TildaTilsynskoordineringAllev1");


            if (sourceList.Count() != 1) //should only return the one source
                throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");

            try
            {
                result = await sourceList?.First().GetAuditCoordinationAllAsync(req, param.month, param.year, param.filter);

                if (result.Status == StatusEnum.NotFound || result.Status == StatusEnum.Failed || result.Status == StatusEnum.Unknown)
                {
                    result.AuditCoordinations = null;
                }

                var taskList = new List<Task<TildaRegistryEntry>>();

                if (result.AuditCoordinations != null)
                {

                    var distinctList = result.AuditCoordinations.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
                    foreach (var item in distinctList)
                    {
                        taskList.Add(GetOrganizationFromBR(item.ControlObject));
                    }
                }

                await Task.WhenAll(taskList);
                taskList = taskList.GroupBy(x => x.Result.OrganizationNumber).Select(y => y.FirstOrDefault()).ToList();
                foreach (var t in taskList)
                {
                    brResults.Add(t.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (result != null)
            {
                var filtered = (AuditCoordinationList)Helpers.Filter(result, brResults);
                ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }
        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynsRapportAllAsync(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = SourcesHelper.GetRelevantSources<ITildaAuditReportsAll>(req.OrganizationNumber, _client, _logger, _settings);
            AuditReportList result = null;
            var brResults = new List<TildaRegistryEntry>();
            var ecb = new EvidenceBuilder(_metadata, "TildaTilsynsrapportAllev1");


            if (sourceList.Count() != 1) //should only return the one source
                throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");

            try
            {
                result = await sourceList?.First().GetAuditReportsAllAsync(req, param.month, param.year, param.filter);

                if (result.Status == StatusEnum.NotFound || result.Status == StatusEnum.Failed || result.Status == StatusEnum.Unknown)
                {
                    result.AuditReports = null;
                }

                var taskList = new List<Task<TildaRegistryEntry>>();

                /* if (result.AuditReports != null)
                 {
                     var distinctList = result.AuditReports.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
                     foreach (var item in distinctList)
                     {
                         taskList.Add(GetOrganizationFromBR(item.ControlObject));
                     }
                 }

                 await Task.WhenAll(taskList);

                 foreach (var t in taskList)
                 {
                     brResults.Add(t.Result);
                 }*/
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            foreach (var unit in brResults)
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

            if (result != null)
            {
                var filtered = (AuditReportList)Helpers.Filter(result, brResults);
                ecb.AddEvidenceValue($"tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }
        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynsrapport(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var brResultTask = GetOrganizationsFromBR(req.OrganizationNumber, param);

            var taskList = new List<Task<AuditReportList>>();
            try
            {
                foreach (ITildaAuditReports a in SourcesHelper.GetRelevantSources<ITildaAuditReports>(param.sourceFilter, _client, _logger, _settings))
                {
                    taskList.Add(a.GetAuditReportsAsync(req, param.fromDate, param.toDate));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new EvidenceSourcePermanentClientException(1,"Could not create requests for specified sources");
            }

            await Task.WhenAll(taskList);
            var brResult = await brResultTask;
            var list = new List<AuditReportList>();

            foreach (var task in taskList)
            {
                var values = task.Result;

                if (values.Status == StatusEnum.NotFound || values.Status == StatusEnum.Failed || values.Status == StatusEnum.Unknown)
                {
                    values.AuditReports = null;
                }

                list.Add(values);
            }

            var ecb = new EvidenceBuilder(_metadata, "TildaTilsynsrapportv1");

            foreach (var unit in brResult)
                ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

            foreach (var a in list)
            {
                var filtered = (AuditReportList)Helpers.Filter(a, brResult);
                ecb.AddEvidenceValue($"tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }


    }
}
