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
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class AuditReportFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata,
    IFilterService filterService) : AuditFunctionsBase(brregService)
{
    private ILogger logger;

    [Function("TildaTilsynsrapportv1")]
    public async Task<HttpResponseData> Tilsynsrapport(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportv1")] HttpRequestData req, FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynsrapport(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    [Function("TildaTilsynsrapportAllev1")]
    public async Task<HttpResponseData> TilsynsrapportAlle(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportAllev1")] HttpRequestData req, FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTilsynsRapportAllAsync(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynsrapport(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var brResultTask = GetOrganizationsFromBr(req.OrganizationNumber);

        var taskList = new List<Task<AuditReportList>>();
        try
        {
            foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaAuditReports>(param.sourceFilter))
            {
                taskList.Add(a.GetAuditReportsAsync(req, param.fromDate, param.toDate));
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{exMessage}", ex.Message);
            throw new EvidenceSourcePermanentClientException(1,"Could not create requests for specified sources");
        }

        await Task.WhenAll(taskList);
        var brResult = await brResultTask;
        var list = new List<AuditReportList>();

        foreach (var values in taskList.Select(task => task.Result))
        {
            if (values.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                values.AuditReports = null;
            }

            list.Add(values);
        }

        var ecb = new EvidenceBuilder(metadata, "TildaTilsynsrapportv1");

        foreach (var unit in brResult)
        {
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
        }

        foreach (var a in list)
        {
            var filtered = (AuditReportList)filterService.FilterAuditList(a, brResult);
            ecb.AddEvidenceValue($"tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
        }

        var result = ecb.GetEvidenceValues();

        return result;
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynsRapportAllAsync(EvidenceHarvesterRequest req,
        TildaParameters param)
    {
        var sourceList = tildaSourceProvider.GetRelevantSources<ITildaAuditReportsAll>(req.OrganizationNumber);
        AuditReportList result = null;
        var brResults = new List<TildaRegistryEntry>();
        var ecb = new EvidenceBuilder(metadata, "TildaTilsynsrapportAllev1");


        var tildaAuditReportsAlls = sourceList.ToList();
        //should only return the one source
        if (tildaAuditReportsAlls.Count != 1)
        {
            throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");
        }


        try
        {
            result = await tildaAuditReportsAlls.First().GetAuditReportsAllAsync(req, param.month, param.year, param.filter);

            if (result.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                result.AuditReports = null;
            }

            var taskList = new List<Task<TildaRegistryEntry>>();

            if (result.AuditReports != null)
            {
                var distinctList = result.AuditReports.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault())
                    .ToList();
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
                        logger.LogError(task.Exception, task.Exception?.Message);
                    }

                    taskList = taskList.Where(task => !task.IsFaulted).ToList();
                }
            }

            taskList = taskList
                .Where(task => task.Result is not null)
                .ToList();
            brResults.AddRange(taskList.Select(t => t.Result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        if (result == null)
        {
            return ecb.GetEvidenceValues();
        }

        if (param.HasGeoSearchParams())
        {
            var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToList();
            result.AuditReports =
                result.AuditReports?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
        }

        var filtered = (AuditReportList)filterService.FilterAuditList(result, brResults);
        ecb.AddEvidenceValue("tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None),
            result.ControlAgency, false);

        return ecb.GetEvidenceValues();
    }
}
