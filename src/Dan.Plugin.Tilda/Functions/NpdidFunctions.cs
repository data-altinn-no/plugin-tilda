using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class NpdidFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata,
    IFilterService filterService) : AuditFunctionsBase(brregService)
{
    private ILogger logger;

    [Function("TildaNPDIDv1")]
    public async Task<HttpResponseData> Npdid(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaNPDIDv1")] HttpRequestData req,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesNpdid(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesNpdid(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var brResultTask = GetOrganizationsFromBr(req.OrganizationNumber);

        var taskList = new List<Task<NpdidAuditReportList>>();
        try
        {
            foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaNPDIDAuditReports>(param.sourceFilter))
            {
                taskList.Add(a.GetNPDIDAuditReportsAsync(req, param.fromDate, param.toDate, param.npdid));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new EvidenceSourcePermanentClientException(1, $"Could not create requests for specified sources ({ex.Message}");
        }

        await Task.WhenAll(taskList);
        var brResult = await brResultTask;
        var list = new List<NpdidAuditReportList>();

        foreach (var task in taskList)
        {
            var values = task.Result;

            if (values.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                values.AuditReports = null;
            }

            list.Add(values);
        }

        var ecb = new EvidenceBuilder(metadata, "TildaNPDIDv1");

        foreach (var unit in brResult)
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
        try
        {
            foreach (var a in list)
            {
                var filtered = (NpdidAuditReportList)filterService.FilterAuditList(a, brResult);
                ecb.AddEvidenceValue("tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return ecb.GetEvidenceValues();
    }
}
