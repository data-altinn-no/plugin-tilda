using Azure.Core;
using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.TIlda.Utils;
using Dan.Tilda.Models.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Services;

public interface IBrregService
{
    Task<AccountsInformation> GetAnnualTurnoverFromBr(string organizationNumber);

    Task<List<BREntityRegisterEntry>> GetFromBr(string organization, bool? includeSubunits, bool skipCache = false);

    Task<List<string>> GetKofuviAddresses(string organizationNumber);

    Task<IEnumerable<AccountsInformationYear>> GetAnnualAccountsFromBr(string organizationNumber);
}

public class BrregService(
    IHttpClientFactory httpClientFactory,
    IDistributedCache cache,
    IOptions<Settings> settings,
    ILogger<BrregService> logger) : IBrregService
{
    private readonly HttpClient _safeHttpClient = httpClientFactory.CreateClient("SafeHttpClient");
    private readonly HttpClient _erClient = httpClientFactory.CreateClient("ERHttpClient");
    private readonly HttpClient _kofuviClient = httpClientFactory.CreateClient("KofuviClient");
    private readonly Settings _settings = settings.Value;
    private readonly HttpClient _accountsClient = httpClientFactory.CreateClient();

    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(60);

    public async Task<AccountsInformation> GetAnnualTurnoverFromBr(string organizationNumber)
    {
        var result = new AccountsInformation();
        try
        {
            var accountsUrl = $"http://data.brreg.no/regnskapsregisteret/regnskap/{organizationNumber}";
            var cacheKey = $"Tilda-Cache_Absolute_GET_{accountsUrl}";

            try
            {
                result = await cache.GetValueAsync<AccountsInformation>(cacheKey);
                if (result is not null)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to get AnnualTurnover from cache for {orgNumber}, fetching from source directly. Exception message: {message}",
                    organizationNumber,
                    e.Message);
            }


            var response = await _safeHttpClient.GetAsync(accountsUrl);
            var rawResult = await response.Content.ReadAsStringAsync();

            var tmp = JsonConvert.DeserializeObject<JArray>(rawResult);
            if (tmp is null || tmp.Count == 0)
            {
                return result;
            }

            var entry = tmp[0];
            result = new AccountsInformation()
            {
                ToDate = entry["regnskapsperiode"]?["tilDato"]?.ToObject<DateTime>() ?? default,
                FromDate = entry["regnskapsperiode"]?["fraDato"]?.ToObject<DateTime>() ?? default,
                AnnualTurnover = entry["resultatregnskapResultat"]?["driftsresultat"]?["driftsinntekter"]?["sumDriftsinntekter"]?.ToString(),
                AnnualResult = entry["resultatregnskapResultat"]?["driftsresultat"]?["driftsresultat"]?.ToString(),
                CurrentAssets = entry["eiendeler"]?["omloepsmidler"]?["sumOmloepsmidler"]?.ToString(),
                EarnedEquity = entry["egenkapitalGjeld"]?["egenkapital"]?["opptjentEgenkapital"]?["sumOpptjentEgenkapital"]?.ToString(),
                TotalDebt = entry["egenkapitalGjeld"]?["gjeldOversikt"]?["sumGjeld"]?.ToString(),
                ShortTermDebt = entry["egenkapitalGjeld"]?["gjeldOversikt"]?["kortsiktigGjeld"]?["sumKortsiktigGjeld"]?.ToString(),
                TotalEquity = entry["egenkapitalGjeld"]?["egenkapital"]?["sumEgenkapital"]?.ToString()
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            };
            await cache.SetValueAsync(cacheKey, result, cacheOptions);
        }
        catch (Exception e)
        {
            //Ignore
            logger.LogDebug("Failed to get AnnualTurnover for {orgNumber}. Exception message: {message}",
                organizationNumber,
                e.Message);
        }

        return result;
    }

    public async Task<IEnumerable<AccountsInformationYear>> GetAnnualAccountsFromBr(string organizationNumber)
    {
        var result = new List<AccountsInformationYear>();            

        try
        {
            var annualAccounts = await GetAnnualAccountsForYear(organizationNumber);
            if (annualAccounts is not null)
            {
                result.AddRange(annualAccounts);
            }
        }
        catch (Exception e)
        {
            logger.LogDebug("Failed to get AnnualAccounts for {orgNumber}. Exception message: {message}",
                organizationNumber,
                e.Message);
            return result;
        }        

        return result;
    }

    private async Task<List<AccountsInformationYear>> GetAnnualAccountsForYear(string organizationNumber)
    {
        var accountsUrl = $"https://data.brreg.no/regnskapsregisteret/regnskap/{organizationNumber}";
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, accountsUrl);

        var authenticationString = $"{_settings.RRUserName}:{_settings.RRPassword}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(authenticationString));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

        var response = await _safeHttpClient.SendAsync(requestMessage);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
          
            var regnskaper = JsonConvert.DeserializeObject<List<BrregRegnskap>>(data);
            return regnskaper?.Select(MapToAccountsInformationYear).ToList();
        }
        else
            return null;
    }


    public async Task<List<BREntityRegisterEntry>> GetFromBr(string organization, bool? includeSubunits, bool skipCache = false)
    {
        var result = new List<BREntityRegisterEntry>();

        if (organization is "111111111" or "811105562")
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
            result.AddRange(await GetAllUnitsFromBr(organization, skipCache));
        }
        else
        {
            result.Add(await GetOrganizationInfoFromBr(organization, skipCache));
        }

        return result;
    }

    public async Task<List<string>> GetKofuviAddresses(string organizationNumber)
    {
        var targetUrl = $"{_settings.KofuviEndpoint}/api/varslingsadresser/{organizationNumber}";
        var cacheKey = $"Tilda-Cache_Absolute_GET_{targetUrl}";
        var result = await cache.GetValueAsync<List<string>>(cacheKey);
        if (result is not null)
        {
            return result;
        }

        string responseString;
        try
        {
            var response = await _kofuviClient.GetAsync(targetUrl);
            if (!response.IsSuccessStatusCode)
            {
                //skip logging 404 - just means there are no addresses for this organization and clutters logs
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    logger.LogError(
                        "Failed to get kofuvi addresses for org={organizationNumber} on url={targetUrl} with unsuccessful response status={statusCode}",
                        organizationNumber, targetUrl, response.StatusCode
                    );
                }
                result = [];
                var errorCacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                await cache.SetValueAsync(cacheKey, result, errorCacheOptions);
                return result;
            }
            responseString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Failed to get kofuvi addresses for org={organizationNumber} on url={targetUrl} with exception ex={ex} message={message} status={status}",
                organizationNumber, targetUrl, ex.GetType().Name, ex.Message, "hardfail"
            );
            result = [];
            var errorCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            await cache.SetValueAsync(cacheKey, result, errorCacheOptions);
            return result;
        }

        try
        {
            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            };
            var kofuviResponse = JsonConvert.DeserializeObject<KofuviResponse>(responseString);
            var addresses = kofuviResponse.Embedded.Notification.NotificationAddresses;
            if (addresses is null || addresses.Count == 0)
            {
                result = [];
                await cache.SetValueAsync(cacheKey, result, cacheOptions);
                return result;
            }

            result = addresses
                .Select(a => a.ContactInformation?.DigitalNotificationInformation?.NotificationEmail?.CompleteEmail)
                .Where(a => a is not null)
                .ToList();
            await cache.SetValueAsync(cacheKey, result, cacheOptions);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Failed to deserialize kofuvi response for org={organizationNumber} with exception ex={ex} message={message} status={status}",
                organizationNumber, ex.GetType().Name, ex.Message, "hardfail"
            );
            result = [];
            var errorCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            await cache.SetValueAsync(cacheKey, result, errorCacheOptions);
            return result;
        }
    }

    private async Task<BREntityRegisterEntry> GetOrganizationInfoFromBr(string organizationNumber, bool skipCache = false)
    {
        var mainUnitUrl = $"https://data.brreg.no/enhetsregisteret/api/enheter/{organizationNumber}";
        var subUnitUrl = $"https://data.brreg.no/enhetsregisteret/api/underenheter/{organizationNumber}";
        var cacheKey = $"Tilda-Cache_Absolute_GET_{mainUnitUrl}";
        BREntityRegisterEntry result;

        string rawResult;
        try
        {
            if (!skipCache)
            {
                try
                {
                    result = await cache.GetValueAsync<BREntityRegisterEntry>(cacheKey);
                    if (result is not null)
                    {
                        return result;
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning("Failed to get GetOrganizationInfoFromBr from cache for {orgNumber}, fetching from source directly. Exception message: {message}",
                        organizationNumber,
                        e.Message);
                }
            }

            var response = await _erClient.GetAsync(mainUnitUrl);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response = await _erClient.GetAsync(subUnitUrl);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EvidenceSourcePermanentClientException(
                        Metadata.ERROR_ORGANIZATION_NOT_FOUND,
                        $"{organizationNumber} was not found in the Central Coordinating Register for Legal Entities");
                }
            }

            rawResult = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR, null, e);
        }

        try
        {
            result = JsonConvert.DeserializeObject<BREntityRegisterEntry>(rawResult);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            };
            await cache.SetValueAsync(cacheKey, result, cacheOptions);
        }
        catch
        {
            throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                "Did not understand the data model returned from upstream source");
        }

        return result;
    }

    private async Task<List<BREntityRegisterEntry>> GetAllUnitsFromBr(string organizationNumber, bool skipCache = false)
    {
        List<BREntityRegisterEntry> result;
        string rawResult;
        var url = $"https://data.brreg.no/enhetsregisteret/api/enheter/?overordnetEnhet={organizationNumber}";
        var cacheKey = $"Tilda-Cache_Absolute_GET_{url}";

        try
        {
            if (!skipCache)
            {
                try
                {
                    result = await cache.GetValueAsync<List<BREntityRegisterEntry>>(cacheKey);
                    if (result is not null)
                    {
                        return result;
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning("Failed to get GetAllUnitsFromBr from cache for {orgNumber}, fetching from source directly. Exception message: {message}",
                        organizationNumber,
                        e.Message);
                }
            }

            result = [];
            var response = await _erClient.GetAsync(url);
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
            var parsed = JsonConvert.DeserializeObject<List<BREntityRegisterEntry>>(rawResult);
            result.AddRange(parsed);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            };
            await cache.SetValueAsync(cacheKey, result, cacheOptions);
        }
        catch
        {
            throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                "Did not understand the data model returned from upstream source");
        }

        return result;
    }

    private static AccountsInformationYear MapToAccountsInformationYear(BrregRegnskap r)
    {
        var dr = r.ResultatregnskapResultat?.Driftsresultat;
        var fr = r.ResultatregnskapResultat?.Finansresultat;
        var ei = r.Eiendeler;
        var ek = r.EgenkapitalGjeld?.Egenkapital;
        var gj = r.EgenkapitalGjeld?.GjeldOversikt;

        return new AccountsInformationYear
        {
            FraDato = r.Regnskapsperiode?.FraDato ?? DateTime.MinValue,
            TilDato = r.Regnskapsperiode?.TilDato ?? DateTime.MinValue,

            Salgsinntekter = dr?.Driftsinntekter?.Salgsinntekter ?? 0,
            SumDriftsinntekter = dr?.Driftsinntekter?.SumDriftsinntekter ?? 0,
            Loennskostnad = dr?.Driftskostnad?.Loennskostnad ?? 0,
            SumDriftskostnad = dr?.Driftskostnad?.SumDriftskostnad ?? 0,
            Driftsresultat = dr?.Driftsresultat ?? 0,

            SumFinansinntekter = fr?.Finansinntekt?.SumFinansinntekter ?? 0,
            RentekostnadSammeKonsern = fr?.Finanskostnad?.RentekostnadSammeKonsern ?? 0,
            AnnenRentekostnad = fr?.Finanskostnad?.AnnenRentekostnad ?? 0,
            SumFinanskostnad = fr?.Finanskostnad?.SumFinanskostnad ?? 0,
            NettoFinans = fr?.NettoFinans ?? 0,

            OrdinaertResultatFoerSkattekostnad = r.ResultatregnskapResultat?.OrdinaertResultatFoerSkattekostnad ?? 0,
            OrdinaertResultatSkattekostnad = r.ResultatregnskapResultat?.OrdinaertResultatSkattekostnad ?? 0,
            EkstraordinaerePoster = r.ResultatregnskapResultat?.EkstraordinaerePoster ?? 0,
            SkattekostnadEkstraordinaertResultat = r.ResultatregnskapResultat?.SkattekostnadEkstraordinaertResultat ?? 0,
            Aarsresultat = r.ResultatregnskapResultat?.Aarsresultat ?? 0,
            Totalresultat = r.ResultatregnskapResultat?.Totalresultat ?? 0,

            Goodwill = ei?.Goodwill ?? 0,
            SumAnleggsmidler = ei?.Anleggsmidler?.SumAnleggsmidler ?? 0,
            SumVarer = ei?.SumVarer ?? 0,
            SumFordringer = ei?.SumFordringer ?? 0,
            SumInvesteringer = ei?.SumInvesteringer ?? 0,
            SumBankinnskuddOgKontanter = ei?.SumBankinnskuddOgKontanter ?? 0,
            SumOmloepsmidler = ei?.Omloepsmidler?.SumOmloepsmidler ?? 0,
            SumEiendeler = ei?.SumEiendeler ?? 0,

            SumInnskuttEgenkapital = ek?.InnskuttEgenkapital?.SumInnskuttEgenkaptial ?? 0,
            SumOpptjentEgenkapital = ek?.OpptjentEgenkapital?.SumOpptjentEgenkapital ?? 0,
            SumEgenkapital = ek?.SumEgenkapital ?? 0,
            SumLangsiktigGjeld = gj?.LangsiktigGjeld?.SumLangsiktigGjeld ?? 0,
            SumKortsiktigGjeld = gj?.KortsiktigGjeld?.SumKortsiktigGjeld ?? 0,
            SumGjeld = gj?.SumGjeld ?? 0,
            SumEgenkapitalGjeld = r.EgenkapitalGjeld?.SumEgenkapitalGjeld ?? 0,
        };
    }
}
