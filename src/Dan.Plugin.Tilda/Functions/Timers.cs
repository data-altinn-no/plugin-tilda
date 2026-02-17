using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Exceptions;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Dan.Tilda.Models.Alerts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dan.Plugin.Tilda.Functions;

public class Timers(
    ITildaSourceProvider tildaSourceProvider,
    IMtamCounterClient mtamCounterClient,
    IAlertMessageSender alertMessageSender,
    IConnectionMultiplexer connectionMultiplexer,
    IBrregService brregService)
{
    private ILogger logger;

    // MTAM - Melding til annen myndighet (message to other authority/auditor)
    [Function("MtamTimer")]
    public async Task MessageToOtherAuditorsTimer(
        // [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, // easier to test locally using http trigger
        [TimerTrigger("%MtamTriggerCron%")] TimerInfo timerInfo,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var mtamSources = tildaSourceProvider.GetAllSources<ITildaAlertMessage>().ToList();
        foreach (var mtamSource in mtamSources)
        {
            var mtamCounter = await mtamCounterClient.GetMtamCounter(mtamSource.OrganizationNumber);
            var from = mtamCounter.LastFetched.ToString("yyyy-MM-ddTHH:mm:ss");
            List<AlertSourceMessage> messages;
            try
            {
                messages = await mtamSource.GetAlertMessagesAsync(from);
            }
            catch (FailedToFetchDataException e)
            {
                // If we fail to fetch alert messages from a source, we should continue with the rest of sources
                // but not update this source's counter so we'll try to fetch from the same time again next attempt
                continue;
            }

            var fetched = DateTime.UtcNow;

            foreach (var message in messages)
            {
                await alertMessageSender.Send(mtamSource.OrganizationNumber, message);
            }

            mtamCounter.LastFetched = fetched;
            await mtamCounterClient.UpsertMtamCounter(mtamCounter);
        }
    }

    [Function("CacheRefreshTimer")]
    public async Task CacheRefreshTimer(
        //[HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, // easier to test locally using http trigger
        [TimerTrigger("%CacheRefreshCron%")] TimerInfo timerInfo,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);

        var db = connectionMultiplexer.GetDatabase();
        var now =  DateTime.UtcNow;
        var mainunitKeys = new List<string>();
        var subunitKeys = new List<string>();
        const string mainKeyPrefix = "Tilda-Cache_Absolute_GET_https://data.brreg.no/enhetsregisteret/api/enheter/";
        const string subunitKeyPrefix = "Tilda-Cache_Absolute_GET_https://data.brreg.no/enhetsregisteret/api/enheter/?overordnetEnhet=";

        var keys = GetKeysAsync("Tilda-Cache_Absolute*");
        await foreach (var key in keys)
        {
            var expiry = await db.KeyExpireTimeAsync(key);
            var timeLeft = expiry - now;
            if (!(timeLeft <= TimeSpan.FromMinutes(11)))
            {
                continue;
            }

            if (key.StartsWith(subunitKeyPrefix))
            {
                var subKey =  key.Replace(subunitKeyPrefix, "");
                subunitKeys.Add(subKey);
            }
            else if (key.StartsWith(mainKeyPrefix))
            {
                var mainKey = key.Replace(mainKeyPrefix, "");
                mainunitKeys.Add(mainKey);
            }
        }

        if (mainunitKeys.Count == 0 && subunitKeys.Count == 0)
        {
            return;
        }

        logger.LogInformation("Refreshing {mainKeys} main keys and {subunitKeys} subunit keys", mainunitKeys.Count, subunitKeys.Count);

        var tasks = new List<Task>();
        foreach (var key in mainunitKeys)
        {
            tasks.Add(RefreshOrganisationEntry(key, includeSubunits: false));
        }

        foreach (var key in subunitKeys)
        {
            tasks.Add(RefreshOrganisationEntry(key, includeSubunits:true));
        }

        await Task.WhenAll(tasks);
    }

    private async IAsyncEnumerable<string> GetKeysAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(pattern));
        }

        foreach (var endpoint in connectionMultiplexer.GetEndPoints())
        {
            var server = connectionMultiplexer.GetServer(endpoint);
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                yield return key.ToString();
            }
        }
    }

    private async Task RefreshOrganisationEntry(string org, bool includeSubunits)
    {
        try
        {
            await brregService.GetFromBr(org, includeSubunits, skipCache: true);
        }
        catch (Exception e)
        {
            // Logging warning would be flooding the alerts/dashboards a bit too much, it's not really an issue if
            // something fails to refresh, this is just a feature to prevent regular calls being too long too often
            logger.LogInformation("Failed to refresh organisation cache entry for {org}, exception message: {exMessage}", org, e.Message);
        }
    }
}
