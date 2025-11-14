using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Services;

public interface IBrregService
{
    Task<AccountsInformation> GetAnnualTurnoverFromBr(string organizationNumber);

    Task<List<BREntityRegisterEntry>> GetFromBr(string organization, bool? includeSubunits);

    Task<List<string>> GetKofuviAddresses(string organizationNumber);
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

            result = new AccountsInformation();
            var response = await _safeHttpClient.GetAsync(accountsUrl);
            var rawResult = await response.Content.ReadAsStringAsync();

            dynamic tmp = JsonConvert.DeserializeObject(rawResult);
            if (tmp is null)
            {
                return result;
            }


            result.ToDate = tmp[0]["regnskapsperiode"]["tilDato"];
            result.FromDate = tmp[0]["regnskapsperiode"]["fraDato"];
            result.AnnualTurnover = tmp[0]["resultatregnskapResultat"]["driftsresultat"]["driftsinntekter"]["sumDriftsinntekter"];

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

    public async Task<List<BREntityRegisterEntry>> GetFromBr(string organization, bool? includeSubunits)
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
            result.AddRange(await GetAllUnitsFromBr(organization));
        }
        else
        {
            result.Add(await GetOrganizationInfoFromBr(organization));
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

    private async Task<BREntityRegisterEntry> GetOrganizationInfoFromBr(string organizationNumber)
    {
        var mainUnitUrl = $"https://data.brreg.no/enhetsregisteret/api/enheter/{organizationNumber}";
        var subUnitUrl = $"https://data.brreg.no/enhetsregisteret/api/underenheter/{organizationNumber}";
        var cacheKey = $"Tilda-Cache_Absolute_GET_{mainUnitUrl}";
        BREntityRegisterEntry result;

        string rawResult;
        try
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

    private async Task<List<BREntityRegisterEntry>> GetAllUnitsFromBr(string organizationNumber)
    {
        List<BREntityRegisterEntry> result;
        string rawResult;
        var url = $"https://data.brreg.no/enhetsregisteret/api/enheter/?overordnetEnhet={organizationNumber}";
        var cacheKey = $"Tilda-Cache_Absolute_GET_{url}";

        try
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
}
