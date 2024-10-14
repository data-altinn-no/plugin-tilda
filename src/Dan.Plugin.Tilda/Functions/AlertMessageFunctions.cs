using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Mappers;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Models.AlertMessages;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class AlertMessageFunctions
{
    private readonly ITildaSourceProvider _tildaSourceProvider;
    private readonly IEvidenceService _evidenceService;
    private readonly IAltinnEventClient _altinnEventClient;
    private readonly IAlertMessageMapper _alertMessageMapper;
    private ILogger _logger;

    public AlertMessageFunctions(
        ITildaSourceProvider tildaSourceProvider,
        IEvidenceService evidenceService,
        IAltinnEventClient altinnEventClient,
        IAlertMessageMapper alertMessageMapper)
    {
        _tildaSourceProvider = tildaSourceProvider;
        _evidenceService = evidenceService;
        _altinnEventClient = altinnEventClient;
        _alertMessageMapper = alertMessageMapper;
    }

    // is this even something we should expose? or should we just do it as a support-ops task?
    [Function("CreateMtamSubscription")]
    public async Task<HttpResponseData> CreateMtamSubscription(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mtam/subscriptions")] HttpRequestData req,
        FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        if (evidenceHarvesterRequest.TryGetParameter("endpoint", out string endpoint))
        {
            // TODO: throw appropriate exception
            throw new Exception();
        }

        //TODO: add check if subscription already exists. How do we do this? Not sure, need to investigate what
        // we can control in the altinn events subscription to identify a subscription between the different
        // authorities. Do this in create subscription, or here?
        await _altinnEventClient.CreateSubscription(endpoint, PluginConstants.MtamResourceId);
        throw new NotImplementedException();
    }

    [Function("TildaMeldingTilAnnenMyndighetv1")]
    public async Task<HttpResponseData> TildaMeldingTilAnnenMyndighetv1([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaMeldingTilAnnenMyndighetv1")] HttpRequestData req, FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesMeldingTilAnnenMyndighet(evidenceHarvesterRequest, evidenceHarvesterRequest.GetValuesFromParameters()));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesMeldingTilAnnenMyndighet(EvidenceHarvesterRequest req, TildaParameters param)
    {
        AlertSourceMessage message;
        try
        {
            // Should only be one source, null if not found, if more then we have misconfigured something
            var source = _tildaSourceProvider.GetRelevantSources<ITildaAlertMessage>(param.sourceFilter).SingleOrDefault();
            if (source == null)
            {
                throw new EvidenceSourcePermanentClientException(1, $"Could not find alert message source for {param.sourceFilter}");
            }

            message = await source.GetAlertMessageAsync(req, param.identifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching alert messages: {message}",ex.Message);
            throw new EvidenceSourcePermanentClientException(1, $"Could not create requests for specified sources ({ex.Message}");
        }

        var mappedMessage = _alertMessageMapper.Map(message, param.sourceFilter);

        return _evidenceService.BuildEvidenceValue("TildaMeldingTilAnnenMyndighetv1", "meldingTilAnnenMyndighet", mappedMessage, param.sourceFilter);
    }
}
