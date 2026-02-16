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
using StackExchange.Redis;

namespace Dan.Plugin.Tilda.Functions;

public class Timers(
    ITildaSourceProvider tildaSourceProvider,
    IMtamCounterClient mtamCounterClient,
    IAlertMessageSender alertMessageSender,
    IConnectionMultiplexer connectionMultiplexer,
    IBrregService brregService)
{
    // MTAM - Melding til annen myndighet (message to other authority/auditor)
    [Function("MtamTimer")]
    public async Task MessageToOtherAuditorsTimer(
        // [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, // easier to test locally using http trigger
        [TimerTrigger("%MtamTriggerCron%")] TimerInfo timerInfo,
        FunctionContext context)
    {
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
        var db = connectionMultiplexer.GetDatabase();
        var now =  DateTime.UtcNow;
        var mainunitKeys = new List<string>();
        var subunitKeys = new List<string>();
        const string mainKeyPrefix = "Tilda-Cache_Absolute_GET_https://data.brreg.no/enhetsregisteret/api/enheter/";
        const string subunitKeyPrefix = "Tilda-Cache_Absolute_GET_https://data.brreg.no/enhetsregisteret/api/enheter/?overordnetEnhet=";

        var keys = GetKeysAsync("Tilda-Cache_Absolute*");
        await foreach (var key in keys)
        {
            var expiry = db.KeyExpireTime(key);
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

        var tasks = new List<Task>();
        foreach (var key in mainunitKeys)
        {
            tasks.Add(brregService.GetFromBr(key, includeSubunits:false, skipCache: true));
        }

        foreach (var key in subunitKeys)
        {
            tasks.Add(brregService.GetFromBr(key, includeSubunits:true, skipCache: true));
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
}
