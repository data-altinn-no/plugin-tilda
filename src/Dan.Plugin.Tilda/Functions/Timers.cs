using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Exceptions;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models.AlertMessages;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dan.Plugin.Tilda.Functions;

public class Timers(
    ITildaSourceProvider tildaSourceProvider,
    IMtamCounterClient mtamCounterClient,
    IAlertMessageSender alertMessageSender)
{
    // MTAM - Melding til annen myndighet (message to other authority/auditor)
    [Function("MtamTimer")]
    public async Task MessageToOtherAuditorsTimer(
        // [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, // easier to test locally using http trigger
        [TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
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
}
