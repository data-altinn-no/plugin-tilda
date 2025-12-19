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
using Dan.Tilda.Models.Audits.Trend;
using Dan.Tilda.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class TrendReportFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata,
    IFilterService filterService) : AuditFunctionsBase(brregService)
{
    private ILogger logger;

    [Function("TildaTrendrapportv1")]
    public async Task<HttpResponseData> Trend(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTrendrapportv1")] HttpRequestData req,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTrend(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    [Function("TildaTrendrapportAllev1")]
    public async Task<HttpResponseData> TrendAlle(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTrendrapportAllev1")] HttpRequestData req,
        FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);
        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTrendAll(evidenceHarvesterRequest, GetValuesFromParameters(evidenceHarvesterRequest)));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTrend(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var brResultTask = GetOrganizationsFromBr(req.OrganizationNumber);

        var taskList = new List<Task<TrendReportList>>();
        try
        {
            foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaTrendReports>(param.sourceFilter))
            {
                taskList.Add( a.GetDataTrendAsync(req, param.fromDate, param.toDate));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        await Task.WhenAll(taskList);
        var brResult = await brResultTask;
        var list = new List<TrendReportList>();

        foreach (var values in taskList.Select(task => task.Result))
        {
            if (values.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                values.TrendReports = null;
            }

            list.Add(values);
        }

        var ecb = new EvidenceBuilder(metadata, "TildaTrendrapportv1");

        foreach (var unit in brResult)
        {
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
        }

        foreach (var a in list)
        {
            var filtered = (TrendReportList)filterService.FilterAuditList(a, brResult);
            ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), a.ControlAgency, false);
        }


        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTrendAll(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var sourceList = tildaSourceProvider.GetRelevantSources<ITildaTrendReportsAll>(req.OrganizationNumber).ToList();
        TrendReportList result = null;
        var brResults = new List<TildaRegistryEntry>();
        var ecb = new EvidenceBuilder(metadata, "TildaTrendrapportAllev1");


        //should only return the one source
        if (sourceList.Count!= 1)
        {
            throw new EvidenceSourcePermanentServerException(1001, $"Angitt kilde ({req.OrganizationNumber}) støtter ikke datasettet");
        }

        try
        {
            result = await sourceList.First().GetDataTrendAllAsync(req, param.month, param.year, param.filter);

            if (result.Status is
                StatusEnum.NotFound or
                StatusEnum.Failed or
                StatusEnum.Unknown)
            {
                result.TrendReports = null;
            }

            var taskList = new List<Task<TildaRegistryEntry>>();

            if (result.TrendReports != null)
            {
                var distinctList = result.TrendReports.GroupBy(x => x.ControlObject).Select(y => y.FirstOrDefault()).ToList();
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
            result.TrendReports =
                result.TrendReports?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
        }
        var filtered = (TrendReportList)filterService.FilterAuditList(result, brResults);
        ecb.AddEvidenceValue($"tilsynstrendrapporter", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);

        return ecb.GetEvidenceValues();
    }
}
