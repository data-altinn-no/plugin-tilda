using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Models.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Plugin.Tilda.Extensions;
using Newtonsoft.Json.Linq;


namespace Dan.Plugin.Tilda.Utils
{
    public static class Helpers
    {
        private static OperationStatus GetOperationalStatus(BREntityRegisterEntry brData)
        {
            if (brData.Konkurs)
                return OperationStatus.Konkurs;

            else if (brData.UnderAvvikling)
                return OperationStatus.UnderAvvikling;
            else if (brData.UnderTvangsavviklingEllerTvangsopplosning)
                return OperationStatus.UnderTvangsavviklingEllerTvangsopplosning;
            else
            {
                return OperationStatus.OK;
            }
        }

        public static AuditReportList GetEmptyResponseAuditReportList(string controlAgency)
        {
            return new AuditReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static AuditReportList GetEmptyFailedResponseAuditReportList(string controlAgency)
        {
            return new AuditReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
        {
            string apiUrl = $"{baseUri}/{dataset}/{organizationNumber}?requestor={requestor}";

            if (fromDate != null)
            {
                apiUrl += $"&fromDate={((DateTime)fromDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", System.Globalization.CultureInfo.CurrentCulture)}";
            }

            if (toDate != null)
            {
                apiUrl += $"&toDate={((DateTime)toDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", System.Globalization.CultureInfo.CurrentCulture)}";
            }

            if (!string.IsNullOrEmpty(npdid))
            {
                apiUrl += $"&npdid={npdid}";
            }

            if (!string.IsNullOrEmpty(identifier))
            {
                apiUrl += $"&id={identifier}";
            }

            return apiUrl;
        }

        public static string GetUriAll(string baseUri, string dataset, string requestor, string month, string year, string identifier ="",string npdid = "", string filter = "")
        {
            string apiUrl = $"{baseUri}/{dataset}?requestor={requestor}";

            if (!string.IsNullOrEmpty(month))
            {
                apiUrl += $"&maaned={month}";
            }

            if (!string.IsNullOrEmpty(year))
            {
                apiUrl += $"&aar={year}";
            }

            if (!string.IsNullOrEmpty(npdid))
            {
                apiUrl += $"&npdid={npdid}";
            }

            if (!string.IsNullOrEmpty(identifier))
            {
                apiUrl += $"&id={identifier}";
            }

            if (!string.IsNullOrEmpty(filter))
                apiUrl += $"&filter={HttpUtility.UrlEncode(filter)}";

            return apiUrl;
        }

        public static NPDIDAuditReportList GetEmptyResponseNPDIDAuditReportList(string controlAgency)
        {
            return new NPDIDAuditReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static NPDIDAuditReportList GetEmptyFailedResponseNPDIDAuditReportList(string controlAgency)
        {
            return new NPDIDAuditReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static TrendReportList GetEmptyResponseTrendReportList(string controlAgency)
        {
            return new TrendReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static TrendReportList GetEmptyFailedResponseTrendReportList(string controlAgency)
        {
            return new TrendReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static AuditCoordinationList GetEmptyResponseCoordinationList(string controlAgency)
        {
            return new AuditCoordinationList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static AuditCoordinationList GetEmptyFailedResponseCoordinationList(string controlAgency,
            string statusText = "")
        {
            return new AuditCoordinationList(controlAgency)
            {
                StatusText = statusText,
                Status = StatusEnum.Failed
            };
        }

        private static async Task<List<BREntityRegisterEntry>> GetAllUnitsFromBR(string organizationNumber, HttpClient client, IPolicyRegistry<string> policyRegistry)
        {
            var result = new List<BREntityRegisterEntry>();
            string rawResult;
            try
            {
                var response = await client.GetAsync($"http://data.brreg.no/enhetsregisteret/api/enheter/?overordnetEnhet={organizationNumber}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EvidenceSourcePermanentClientException(
                            Metadata.ERROR_ORGANIZATION_NOT_FOUND,
                            $"{organizationNumber} was not found in the Central Coordinating Register for Legal Entities");
                }

                rawResult = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR, null, e);
            }

            try
            {
                result.AddRange(JsonConvert.DeserializeObject<List<BREntityRegisterEntry>>(rawResult));
            }
            catch
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                    "Did not understand the data model returned from upstream source");
            }

            return result;
        }

        public static async Task<List<BREntityRegisterEntry>> GetFromBR(string organization, HttpClient client, bool? includeSubunits, IPolicyRegistry<string> policyRegistry)
        {
            List<BREntityRegisterEntry> result = new List<BREntityRegisterEntry>();

            if (organization == "111111111")
            {
                result.Add(new BREntityRegisterEntry()
                {
                    Navn = "TESTORGANISASJON",
                    Organisasjonsnummer = 111111111,
                    Organisasjonsform = new Organisasjonsform() { Kode = "TEST" }
                });

                return result;
            }

            if (includeSubunits == true)
            {
                result.AddRange(await GetAllUnitsFromBR(organization, client, policyRegistry));
            }
            else
            {
                result.Add(await GetOrganizationInfoFromBR(organization, client, policyRegistry));
            }

            return result;
        }

        public static async Task<AccountsInformation> GetAnnualTurnoverFromBR(string organizationNumber, HttpClient client, IPolicyRegistry<string> policyRegistry)
        {
            AccountsInformation result = new AccountsInformation();
            try
            {
                var cachePolicy = policyRegistry.Get<AsyncPolicy<string>>("ERCachePolicy");
                var accountsUrl = $"http://data.brreg.no/regnskapsregisteret/regnskap/{organizationNumber}";

                var cacheKey = $"Cache_Absolute_GET_{accountsUrl}";

                var rawResult = await cachePolicy.ExecuteAsync(async context =>
                    {
                        var response = await client.GetAsync(accountsUrl);
                        return await response.Content.ReadAsStringAsync();
                    },
                    new Context(cacheKey));

                dynamic tmp = JsonConvert.DeserializeObject(rawResult);
                result.ToDate = tmp[0]["regnskapsperiode"]["tilDato"];
                result.FromDate = tmp[0]["regnskapsperiode"]["fraDato"];   
                result.AnnualTurnover = tmp[0]["resultatregnskapResultat"]["driftsresultat"]["driftsinntekter"]["sumDriftsinntekter"];           
            }
            catch
            {
               //Ignore
            }


            return result;
        }

        public static async Task<BREntityRegisterEntry> GetOrganizationInfoFromBR(string organizationNumber, HttpClient client, IPolicyRegistry<string> policyRegistry)
        {
            string rawResult;
            BREntityRegisterEntry result;
            try
            {
                var cachePolicy = policyRegistry.Get<AsyncPolicy<string>>("ERCachePolicy");
                var mainUnitUrl = $"http://data.brreg.no/enhetsregisteret/api/enheter/{organizationNumber}";
                var subUnitUrl = $"http://data.brreg.no/enhetsregisteret/api/underenheter/{organizationNumber}";
                var cacheKey = $"Cache_Absolute_GET_{mainUnitUrl}";

                rawResult = await cachePolicy.ExecuteAsync(async context =>
                {
                    var response = await client.GetAsync(mainUnitUrl);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        response = await client.GetAsync(subUnitUrl);
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new EvidenceSourcePermanentClientException(
                                Metadata.ERROR_ORGANIZATION_NOT_FOUND,
                                $"{organizationNumber} was not found in the Central Coordinating Register for Legal Entities");
                        }
                    }

                    return await response.Content.ReadAsStringAsync();
                },
                new Context(cacheKey));
            }
            catch (HttpRequestException e)
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR, null, e);
            }

            try
            {
                result = JsonConvert.DeserializeObject<BREntityRegisterEntry>(rawResult);
            }
            catch
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                    "Did not understand the data model returned from upstream source");
            }

            return result;
        }

        public static async Task<T> GetData<T>(string url, string sourceOrgNo, HttpClient client, ILogger logger, string mpToken, string requestor) where T : IAuditList, new()
        {

            //For local test purposes
            if (string.IsNullOrEmpty(mpToken))
                mpToken = "NOT SET";
            var resultList = new T();

            using var t = logger.Timer($"{sourceOrgNo}-retrieval");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", "bearer " + mpToken);

                logger.LogInformation("Data retrieval started from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor}",
                    sourceOrgNo, url, requestor);
                var result = await client.SendAsync(request);
                logger.LogInformation(
                    "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "ok");

                if (result.IsSuccessStatusCode)
                {
                    var body = await result.Content.ReadAsStringAsync();
                    resultList = JsonConvert.DeserializeObject<T>(body);
                    if (resultList == null)
                    {
                        resultList = new T();
                        resultList.SetStatusAndTextAndOwner($"OK (empty response with 200). ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.OK, sourceOrgNo);
                        logger.LogWarning(
                            "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                            sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "okwarn");
                    }
                    else resultList.SetStatusAndTextAndOwner($"OK. ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.OK, sourceOrgNo);
                }
                else
                {
                    resultList.SetStatusAndTextAndOwner($"Failed: {result.ReasonPhrase}. ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.Failed,
                        sourceOrgNo);
                    logger.LogWarning(
                        "Data retrieval failed gracefully sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} statusCode={statusCode} reasonPhrase={reasonPhrase} status={status}",
                        sourceOrgNo, url, requestor, t.ElapsedMilliseconds, result.StatusCode.ToString(), result.ReasonPhrase, "softfail"
                    );
                }
            }
            catch (Exception ex)
            {
                resultList ??= new T();
                resultList.SetStatusAndTextAndOwner(
                    $"Failed to retrieve data from {sourceOrgNo} with exception {ex.GetType().Name}: {ex.Message}",
                    StatusEnum.Failed, sourceOrgNo);

                logger.LogError(
                    "Data retrieval failed with exception sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} ex={ex} message={message} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, ex.GetType().Name, ex.Message, "hardfail"
                );
            }

            return resultList;
        }

        public static IAuditList Filter(IAuditList resultList, List<TildaRegistryEntry> orgs)
        {
            // *** TEMPORARY TILDA FILTERING ***
            // We need to temporarily remove _all_ data from the result list if orgForm is ENK
            // For all other orgforms
            // - remove meldingTilAnnenMyndighet 
            // - remove tilsynsnotater
            // - remove kontaktperson name
            if (orgs == null || orgs.Count == 0)
            {
                return resultList;
            }

            var firstOrg = orgs.First();
            // If first organization is a specific test organization, disable filtering
            if (firstOrg.OrganizationNumber == "111111111") 
            {
                return resultList;
            }

            bool IsEnk(string orgNo)
            {
                return orgs.Any(x => x.OrganizationNumber == orgNo && x.OrganisationForm == "ENK");
            }

            switch (resultList) 
            {
                case AuditReportList filteredResultList:
                    filteredResultList.AuditReports?.ForEach(x =>
                    {
                        x.AuditNotes = null;

                        if (IsEnk(x.ControlObject))
                        {
                            x.NotesAndRemarks.Clear();
                            x.ControlAttributes = null;
                        }
                    });
                    return filteredResultList;

                case AuditCoordinationList filteredResultList:
                    filteredResultList.AuditCoordinations?.ForEach(x =>
                    {
                        if (IsEnk(x.ControlObject))
                        {
                            x.Alerts?.Clear();
                        }
                    });
                    return filteredResultList;

                case NPDIDAuditReportList filteredResultList:
                    filteredResultList.AuditReports?.ForEach(x =>
                    {
                        x.AuditNotes = null;

                        if (IsEnk(x.ControlObject))
                        {
                            x.NotesAndRemarks.Clear();
                            x.ControlAttributes = null;
                        }
                    });
                    return filteredResultList;

                case TrendReportList filteredResultList:

                    //Remove all ENKs from trendreport list
                    filteredResultList.TrendReports = filteredResultList.TrendReports?.Where(x => !IsEnk(x.ControlObject)).ToList();

                    return filteredResultList;
            }

            return resultList;
        }

        public static string GetJsonSchemaAsString(JSchema schema)
        {
            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.None;
            schema.WriteTo(jsonWriter);
            return writer.ToString();
        }

        public static async Task<string[]> GetFileContents(string folder, string fileName)
        {
            var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));
            return await File.ReadAllLinesAsync(rootDirectory + $@"\{folder}\{fileName}");           
        }

        public static async Task<List<string>> GetParagraph(string paragraph)
        {
            List<string> resultList = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"p{paragraph}.txt"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var contents = await reader.ReadLineAsync();
                    resultList.Add(FormatLine(contents));
                }
            }

            return resultList;
        }

        public static string FormatLine(this string input)
        {
            return input.Replace(" ", "").Replace(Environment.NewLine, "");
        }

        public static async Task<byte[]> GetPdfreport(string url, string sourceOrgNo, HttpClient client, ILogger logger, string mpToken, string requestor)
        {
            //For local test purposes
            if (string.IsNullOrEmpty(mpToken))
                mpToken = "NOT SET";

            using var t = logger.Timer($"{sourceOrgNo}-retrieval");

            byte[] result = null;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Accept", "application/pdf");
                request.Headers.TryAddWithoutValidation("Authorization", "bearer " + mpToken);

                logger.LogInformation("Data retrieval started from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor}",
                    sourceOrgNo, url, requestor);
                var responseMessage = await client.SendAsync(request);
                logger.LogInformation(
                    "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "ok");

                if (responseMessage.IsSuccessStatusCode)
                {
                    result = await responseMessage.Content.ReadAsByteArrayAsync();


                    if (result.Length == 0)
                    {
                        logger.LogWarning(
                            "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                            sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "okwarn");
                    }
                }
                else
                {
                    logger.LogWarning(
                        "Data retrieval failed gracefully sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} statusCode={statusCode} reasonPhrase={reasonPhrase} status={status}",
                        sourceOrgNo, url, requestor, t.ElapsedMilliseconds, responseMessage.StatusCode.ToString(), responseMessage.ReasonPhrase, "softfail"
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "Data retrieval failed with exception sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} ex={ex} message={message} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, ex.GetType().Name, ex.Message, "hardfail"
                );
            }

            return result;
        }
    }
}
