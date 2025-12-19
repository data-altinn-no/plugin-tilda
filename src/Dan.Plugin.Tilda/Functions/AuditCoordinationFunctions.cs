using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Dan.Tilda.Models.Audits.Coordination;
using Dan.Tilda.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class AuditCoordinationFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata,
    IFilterService filterService) : AuditFunctionsBase(brregService)
{
    private ILogger logger;

    [Function("TildaTilsynskoordineringv1")]
    public async Task<HttpResponseData> Tilsynskoordinering(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynskoordineringv1")] HttpRequestData req,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynskoordinering(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    [Function("TildaTilsynskoordineringAllev1")]
    public async Task<HttpResponseData> TilsynskoordineringAlle(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynskoordineringAllev1")] HttpRequestData req,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynskoordingeringAllASync(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordinering(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var brResultTask = GetOrganizationsFromBr(req.OrganizationNumber);

        var taskList = new List<Task<AuditCoordinationList>>();
        try
        {
            foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaAuditCoordination>(param.sourceFilter))
            {
                taskList.Add(a.GetAuditCoordinationAsync(req, param.fromDate, param.toDate));
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{exMessage}",ex.Message);
        }

        await Task.WhenAll(taskList);
        var brResult = await brResultTask;
        var list = new List<AuditCoordinationList>();

        foreach (var values in taskList.Select(task => task.Result))
        {
            if (values.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                values.AuditCoordinations = null;
            }

            list.Add(values);
        }

        var ecb = new EvidenceBuilder(metadata, "TildaTilsynskoordineringv1");
        foreach (var unit in brResult)
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

        foreach (var filtered in list.Select(a => (AuditCoordinationList)filterService.FilterAuditList(a, brResult)))
        {
            ecb.AddEvidenceValue("tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), filtered.ControlAgency, false);
        }

        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordingeringAllASync(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var sourceList = tildaSourceProvider.GetRelevantSources<ITildaAuditCoordinationAll>(req.OrganizationNumber).ToList();
        AuditCoordinationList result = null;
        var brResults = new List<TildaRegistryEntry>();
        var ecb = new EvidenceBuilder(metadata, "TildaTilsynskoordineringAllev1");


        //should only return the one source
        if (sourceList.Count != 1)
        {
            throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");
        }

        try
        {
            result = await sourceList.First().GetAuditCoordinationAllAsync(req, param.month, param.year, param.filter);

            if (result.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                result.AuditCoordinations = null;
            }

            var taskList = new List<Task<TildaRegistryEntry>>();

            if (result.AuditCoordinations != null)
            {
                var distinctList = result.AuditCoordinations.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
                taskList.AddRange(distinctList.Select(item => GetOrganizationFromBr(item.ControlObject, param)));
            }

            var taskResult = Task.WhenAll(taskList);
            try
            {
                await taskResult;
            }
            catch (Exception)
            {
                // Don't want one failed fetch to break the listing of the rest of the orgs
                if (taskResult.IsFaulted)
                {
                    var failedTasks = taskList.Where(task => task.IsFaulted).ToList();
                    foreach (var task in failedTasks)
                    {
                        logger.LogError(task.Exception, "{message}", task.Exception?.Message);
                    }
                    taskList = taskList.Where(task => !task.IsFaulted).ToList();
                }
            }
            taskList = taskList
                .Where(task => task.Result is not null)
                .GroupBy(x => x.Result.OrganizationNumber)
                .Select(y => y.FirstOrDefault())
                .ToList();

            brResults.AddRange(taskList.Select(t => t.Result));
        }
        catch (Exception ex)
        {
            logger.LogError("{message}", ex.Message);
        }

        if (result == null)
        {
            return ecb.GetEvidenceValues();
        }

        if (param.HasGeoSearchParams())
        {
            var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToList();
            result.AuditCoordinations =
                result.AuditCoordinations?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
        }
        var filtered = (AuditCoordinationList)filterService.FilterAuditList(result, brResults);
        ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);

        return ecb.GetEvidenceValues();
    }
}
