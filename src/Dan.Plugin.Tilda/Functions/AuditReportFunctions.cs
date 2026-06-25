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
using Dan.Tilda.Models.Entities;
using Dan.Tilda.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        var subject = req.SubjectParty.Scheme is null ? req.SubjectParty.Id : req.SubjectParty.NorwegianOrganizationNumber;
        bool npdid = subject.Length < 9; // Assume npdid if less than 9 digits


        var taskList = GetReportListTasks(req, param, npdid);
        await Task.WhenAll(taskList);

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

        // need to get org number from control object if subject is npdid
        var orgs = new List<TildaRegistryEntry>();
        var orgNumber = npdid ?
            list.FirstOrDefault()?
                .AuditReports?
                .FirstOrDefault()?
                .ControlObject :
                subject;
        // Only populate orgs if we have a valid orgNumber
        var orgInfoUnavailable = false;
        if (!string.IsNullOrEmpty(orgNumber))
        {
            var brResultTask = GetOrganizationsFromBr(orgNumber, logger);
            var brResult = await brResultTask;
            orgs = brResult.Organizations;
            orgInfoUnavailable = brResult.OrgInfoUnavailable;
        }

        var ecb = new EvidenceBuilder(metadata, "TildaTilsynsrapportv1");

        foreach (var unit in orgs)
        {
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);
        }

        foreach (var a in list)
        {
            var filtered = (AuditReportList)filterService.FilterAuditList(a, orgs, orgInfoUnavailable);
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
        }
        catch (Exception ex)
        {
            logger.LogError("Failed getting TilsynsRapportAlle for org {OrganizationNumber}: {message}", req.OrganizationNumber, ex.Message);
        }

        if (result == null)
        {
            return ecb.GetEvidenceValues();
        }

        var brResults = new List<TildaRegistryEntry>();
        if (param.HasGeoSearchParams())
        {
            try
            {
                if (result.AuditReports != null)
                {
                    var distinctOrgs = result.AuditReports.Select(x => x.ControlObject).Distinct().ToList();
                    brResults.AddRange(await GetOrganizationsFromBrBounded(distinctOrgs, param, logger));
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed getting TilsynsRapportAlle org data for org {OrganizationNumber}: {message}", req.OrganizationNumber, ex.Message);
            }

            var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToHashSet();
            result.AuditReports =
                result.AuditReports?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
        }

        // If brResults is null or empty, filtered will be the same as it was before
        var filtered = (AuditReportList)filterService.FilterAuditList(result, brResults);
        ecb.AddEvidenceValue("tilsynsrapporter", JsonConvert.SerializeObject(filtered, Formatting.None),
            result.ControlAgency, false);

        return ecb.GetEvidenceValues();
    }

    private List<Task<AuditReportList>> GetReportListTasks(EvidenceHarvesterRequest req, TildaParameters param, bool npdid)
    {
        var taskList = new List<Task<AuditReportList>>();
        try
        {
            if(npdid)
            {
                foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaNPDIDAuditReports>(param.sourceFilter))
                {
                    taskList.Add(a.GetAuditReportsAsync(req, param.fromDate, param.toDate));
                }
            }
            else
            {
                foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaAuditReports>(param.sourceFilter))
                {
                    taskList.Add(a.GetAuditReportsAsync(req, param.fromDate, param.toDate));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{exMessage}", ex.Message);
            throw new EvidenceSourcePermanentClientException(1, "Could not create requests for specified sources");
        }
        return taskList;
    }
}
