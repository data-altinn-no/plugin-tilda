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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Services;
using Microsoft.Extensions.Caching.Distributed;


namespace Dan.Plugin.Tilda
{
    public class Tilda
    {
        private readonly IDistributedCache _cache;
        private ILogger _logger;
        private HttpClient _client;
        private HttpClient _erClient;
        private HttpClient _kofuviClient;
        private Settings _settings;
        private readonly IEntityRegistryService _entityRegistryService;
        private readonly IEvidenceSourceMetadata _metadata;
        private readonly IBrregService _brregService;
        private readonly IFilterService _filterService;
        private List<string> P6Orgs;
        private List<string> P9Orgs;

        private readonly ITildaSourceProvider _tildaSourceProvider;

        public Tilda(
            IHttpClientFactory httpClientFactory,
            IOptions<Settings> settings,
            IDistributedCache cache,
            IEntityRegistryService entityRegistry,
            IEvidenceSourceMetadata metadata,
            ITildaSourceProvider tildaSourceProvider,
            IBrregService brregService,
            IFilterService filterService)
        {
            _cache = cache;
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _erClient = httpClientFactory.CreateClient("ERHttpClient");
            _kofuviClient = httpClientFactory.CreateClient("KofuviClient");
            _settings = settings.Value;
            _entityRegistryService = entityRegistry;
            _entityRegistryService.AllowTestCcrLookup = _settings.IsLocalDevelopment || _settings.IsLocalDevelopment;
            _metadata = metadata;
            _tildaSourceProvider = tildaSourceProvider;
            _brregService = brregService;
            _filterService = filterService;
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
                var pdfTarget = _tildaSourceProvider.GetRelevantSources<ITildaPdfReport>(filter).FirstOrDefault();

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

            var trend = _tildaSourceProvider.GetAllSources<ITildaTrendReports>().Select(x=>x.OrganizationNumber + ":" + x.ControlAgency);
            var trendAll = _tildaSourceProvider.GetAllSources<ITildaTrendReportsAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var audit = _tildaSourceProvider.GetAllSources<ITildaAuditReports>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
            var auditAll = _tildaSourceProvider.GetAllSources<ITildaAuditReportsAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var coordination = _tildaSourceProvider.GetAllSources<ITildaAuditCoordination>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
            var coordinationAll = _tildaSourceProvider.GetAllSources<ITildaAuditCoordinationAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var npdid = _tildaSourceProvider.GetAllSources<ITildaNPDIDAuditReports>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var pdfReport = _tildaSourceProvider.GetAllSources<ITildaPdfReport>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var alertMessages = _tildaSourceProvider.GetAllSources<ITildaAlertMessage>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

            var all = _tildaSourceProvider.GetAllRegisteredSources().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);



            ecb.AddEvidenceValue("TildaTrendrapportv1", string.Join(",", trend), "Tilda", false);
            ecb.AddEvidenceValue("TildaTrendrapportAllev1", string.Join(",", trendAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynsrapportv1", string.Join(",", audit), "Tilda", false);
            ecb.AddEvidenceValue("TildaTilsynsrapportAllev1", string.Join(",", auditAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynskoordineringv1", string.Join(",", coordination), "Tilda", false);
            ecb.AddEvidenceValue("TildaTilsynskoordineringAllev1", string.Join(",", coordinationAll), "Tilda", false);

            ecb.AddEvidenceValue("TildaNPDIDv1", string.Join(",", npdid), "Tilda", false);

            ecb.AddEvidenceValue("TildaTilsynsrapportpdfv1", string.Join(",", pdfReport), "Tilda", false);

            ecb.AddEvidenceValue("TildaMeldingTilAnnenMyndighetv1", string.Join(",", alertMessages), "Tilda", false);

            ecb.AddEvidenceValue("AlleKilder", string.Join(",", all), "Tilda", false);

            return await Task.FromResult(ecb.GetEvidenceValues());
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesNpdid(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var brResultTask = GetOrganizationsFromBR(req.OrganizationNumber, param);

            var taskList = new List<Task<NPDIDAuditReportList>>();
            try
            {
                foreach (ITildaNPDIDAuditReports a in _tildaSourceProvider.GetRelevantSources<ITildaNPDIDAuditReports>(param.sourceFilter))
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
                    var filtered = (NPDIDAuditReportList)_filterService.FilterAuditList(a, brResult);
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
            req.TryGetParameter("postnummer", out string postcode);
            req.TryGetParameter("kommunenummer", out string municipalityNumber);
            req.TryGetParameter("naeringskode", out string nace);

            if (includeSubunits)
                throw new Exception("inkluderUnderenheter er ikke støttet ennå :)");

            return new TildaParameters(fromDateTime, toDateTime, npdid, false, sourceFilter, identifier, filter, year, month, postcode, municipalityNumber, nace);
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
            P6Orgs = await ResourceManager.GetParagraph("6");
            P9Orgs = await ResourceManager.GetParagraph("9");
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
            var brResult = await _brregService.GetFromBr(organizationNumber, false);
            var brEntity = brResult.First();
            AccountsInformation accountsInformation = null;
            if (string.IsNullOrEmpty(brEntity.OverordnetEnhet) && brEntity.Organisasjonsform.Kode != "ENK")
            {
                accountsInformation = await _brregService.GetAnnualTurnoverFromBr(organizationNumber);
            }

            var kofuviAddresses = await _brregService.GetKofuviAddresses(organizationNumber);

            var organization = await ConvertBRtoTilda(brEntity, accountsInformation);
            if (kofuviAddresses.Count > 0)
            {
                organization.Emails = kofuviAddresses;
            }
            result.Add(organization);
            return result;
        }

        private async Task<TildaRegistryEntry> GetOrganizationFromBR(string organizationNumber, TildaParameters tildaParameters = null)
        {
            var brResult = await _brregService.GetFromBr(organizationNumber, false);
            var brEntity = brResult.First();
            AccountsInformation accountsInformation = null;

            // Filters out on parameters, currently only on "geo search" params
            if (!brEntity.MatchesFilterParameters(tildaParameters))
            {
                return null;
            }

            if (brEntity.Organisasjonsform.Kode != "ENK")
            {
                accountsInformation = await _brregService.GetAnnualTurnoverFromBr(organizationNumber);
            }

            var kofuviAddresses = await _brregService.GetKofuviAddresses(organizationNumber);

            var organization = await ConvertBRtoTilda(brEntity, accountsInformation);
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
                foreach (ITildaAuditCoordination a in _tildaSourceProvider.GetRelevantSources<ITildaAuditCoordination>(param.sourceFilter))
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
                var filtered = (AuditCoordinationList)_filterService.FilterAuditList(a, brResult);
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
                foreach (ITildaTrendReports a in _tildaSourceProvider.GetRelevantSources<ITildaTrendReports>(param.sourceFilter))
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
                var filtered = (TrendReportList)_filterService.FilterAuditList(a, brResult);
                ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
            }


            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTrendAll(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = _tildaSourceProvider.GetRelevantSources<ITildaTrendReportsAll>(req.OrganizationNumber);
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
                        taskList.Add(GetOrganizationFromBR(item.ControlObject, param));
                    }
                }

                var taskResult = Task.WhenAll(taskList);
                try
                {
                    await taskResult;
                }
                catch (Exception e)
                {
                    // Don't want one failed fetch to break the listing of the rest of the orgs
                    if (taskResult.IsFaulted)
                    {
                        var failedTasks = taskList.Where(task => task.IsFaulted).ToList();
                        foreach (var task in failedTasks)
                        {
                            _logger.LogError(task.Exception, task.Exception?.Message);
                        }
                        taskList = taskList.Where(task => !task.IsFaulted).ToList();
                    }
                }

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
                if (param.HasGeoSearchParams())
                {
                    var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToList();
                    result.TrendReports =
                        result.TrendReports?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
                }
                var filtered = (TrendReportList)_filterService.FilterAuditList(result, brResults);
                ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordingeringAllASync(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = _tildaSourceProvider.GetRelevantSources<ITildaAuditCoordinationAll>(req.OrganizationNumber).ToList();
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
                        taskList.Add(GetOrganizationFromBR(item.ControlObject, param));
                    }
                }

                var taskResult = Task.WhenAll(taskList);
                try
                {
                    await taskResult;
                }
                catch (Exception e)
                {
                    // Don't want one failed fetch to break the listing of the rest of the orgs
                    if (taskResult.IsFaulted)
                    {
                        var failedTasks = taskList.Where(task => task.IsFaulted).ToList();
                        foreach (var task in failedTasks)
                        {
                            _logger.LogError(task.Exception, task.Exception?.Message);
                        }
                        taskList = taskList.Where(task => !task.IsFaulted).ToList();
                    }
                }
                taskList = taskList
                    .Where(task => task.Result is not null)
                    .GroupBy(x => x.Result.OrganizationNumber)
                    .Select(y => y.FirstOrDefault())
                    .ToList();

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
                if (param.HasGeoSearchParams())
                {
                    var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToList();
                    result.AuditCoordinations =
                        result.AuditCoordinations?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
                }
                var filtered = (AuditCoordinationList)_filterService.FilterAuditList(result, brResults);
                ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);
            }

            return ecb.GetEvidenceValues();
        }
        private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynsRapportAllAsync(EvidenceHarvesterRequest req, TildaParameters param)
        {
            var sourceList = _tildaSourceProvider.GetRelevantSources<ITildaAuditReportsAll>(req.OrganizationNumber);
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

                if (result.AuditReports != null)
                {
                    var distinctList = result.AuditReports.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
                    foreach (var item in distinctList)
                    {
                        taskList.Add(GetOrganizationFromBR(item.ControlObject, param));
                    }
                }

                var taskResult = Task.WhenAll(taskList);
                try
                {
                    await taskResult;
                }
                catch (Exception e)
                {
                    // Don't want one failed fetch to break the listing of the rest of the orgs
                    if (taskResult.IsFaulted)
                    {
                        var failedTasks = taskList.Where(task => task.IsFaulted).ToList();
                        foreach (var task in failedTasks)
                        {
                            _logger.LogError(task.Exception, task.Exception?.Message);
                        }
                        taskList = taskList.Where(task => !task.IsFaulted).ToList();
                    }
                }

                taskList = taskList
                    .Where(task => task.Result is not null)
                    .ToList();
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
                if (param.HasGeoSearchParams())
                {
                    var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToList();
                    result.AuditReports =
                        result.AuditReports?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
                }
                var filtered = (AuditReportList)_filterService.FilterAuditList(result, brResults);
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
                foreach (ITildaAuditReports a in _tildaSourceProvider.GetRelevantSources<ITildaAuditReports>(param.sourceFilter))
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
                var filtered = (AuditReportList)_filterService.FilterAuditList(a, brResult);
                ecb.AddEvidenceValue($"tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
            }

            var result = ecb.GetEvidenceValues();

            return result;
        }
    }
}
