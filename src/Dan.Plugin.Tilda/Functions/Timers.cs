using System;
using System.Linq;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dan.Plugin.Tilda.Functions;

public class Timers
{
    private readonly ITildaSourceProvider _tildaSourceProvider;
    private readonly IMtamCounterClient _mtamCounterClient;
    private readonly IAlertMessageSender _alertMessageSender;

    public Timers(
        ITildaSourceProvider tildaSourceProvider,
        IMtamCounterClient mtamCounterClient,
        IAlertMessageSender alertMessageSender)
    {
        _tildaSourceProvider = tildaSourceProvider;
        _mtamCounterClient = mtamCounterClient;
        _alertMessageSender = alertMessageSender;
    }

    // MTAM - Melding til annen myndighet (message to other authority/auditor)
    [Function("MtamTimer")]
    public async Task MessageToOtherAuditorsTimer(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        // [TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
        FunctionContext context)
    {
        var mtamSources = _tildaSourceProvider.GetAllSources<ITildaAlertMessage>().ToList();
        foreach (var mtamSource in mtamSources)
        {
            var mtamCounter = await _mtamCounterClient.GetMtamCounter(mtamSource.OrganizationNumber);
            var from = mtamCounter.LastFetched.ToString("yyyy-MM-ddTHH:mm:ss");
            var messages = await mtamSource.GetAlertMessagesAsync(from);
            var fetched = DateTime.UtcNow;

            foreach (var message in messages)
            {
                await _alertMessageSender.Send(mtamSource.OrganizationNumber, message);
            }

            mtamCounter.LastFetched = fetched;
            await _mtamCounterClient.UpsertMtamCounter(mtamCounter);
        }
    }
}
