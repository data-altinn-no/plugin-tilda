using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Mappers;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Dan.Tilda.Models.Alerts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class AlertMessageFunctions(
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceService evidenceService,
    IAlertMessageMapper alertMessageMapper)
{
    private ILogger _logger;

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
            var source = tildaSourceProvider.GetRelevantSources<ITildaAlertMessage>(param.sourceFilter).SingleOrDefault();
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

        var mappedMessage = alertMessageMapper.Map(message, param.sourceFilter);

        return evidenceService.BuildEvidenceValue("TildaMeldingTilAnnenMyndighetv1", "default", mappedMessage, param.sourceFilter);
    }
}
