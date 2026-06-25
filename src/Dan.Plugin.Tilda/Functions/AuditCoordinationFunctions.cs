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
        var subject = req.GetTildaSubject();
        bool npdid = subject.Length < 9; // Assume npdid if less than 9 digits

        var taskList = GetReportListTasks(req, param, npdid);
        await Task.WhenAll(taskList);

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

        // If no one supports NPDID, we can basically just return an empty response this early
        // as there is no use in looking more up without even having a valid org number
        if (npdid && list.Count == 0)
        {
            ecb.AddEvidenceValue("enhetsinformasjon", null, "Enhetsregisteret", false);
            ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(new AuditCoordinationList(), Formatting.None), null, false);
            var emptyResult = ecb.GetEvidenceValues();
            return emptyResult;
        }

        // need to get org number from control object if subject is npdid
        var orgs = new List<TildaRegistryEntry>();
        var orgNumber = npdid ?
            list.FirstOrDefault()?
                .AuditCoordinations?
                .FirstOrDefault()?
                .ControlObject :
                subject;
        // Only populate orgs if we have a valid orgNumber
        var orgInfoUnavailable = false;
        if(!string.IsNullOrEmpty(orgNumber))
        {
            var brResultTask = GetOrganizationsFromBr(orgNumber, logger);
            var brResult = await brResultTask;
            orgs = brResult.Organizations;
            orgInfoUnavailable = brResult.OrgInfoUnavailable;
        }

        foreach (var unit in orgs)
            ecb.AddEvidenceValue("enhetsinformasjon", JsonConvert.SerializeObject(unit), "Enhetsregisteret", false);

        foreach (var filtered in list.Select(a => (AuditCoordinationList)filterService.FilterAuditList(a, orgs, orgInfoUnavailable)))
        {
            ecb.AddEvidenceValue("tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), filtered.ControlAgency, false);
        }

        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTilsynskoordingeringAllASync(EvidenceHarvesterRequest req, TildaParameters param)
    {
        var sourceList = tildaSourceProvider.GetRelevantSources<ITildaAuditCoordinationAll>(req.OrganizationNumber).ToList();
        AuditCoordinationList result = null;
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
        }
        catch (Exception ex)
        {
            logger.LogError("Failed getting TilsynsKoordineringAlle for org {OrganizationNumber}: {message}", req.OrganizationNumber, ex.Message);
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
                if (result.AuditCoordinations != null)
                {
                    var distinctOrgs = result.AuditCoordinations.Select(x => x.ControlObject).Distinct().ToList();
                    brResults.AddRange(await GetOrganizationsFromBrBounded(distinctOrgs, param, logger));
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed getting TilsynsKoordineringAlle org info for org {OrganizationNumber}: {message}", req.OrganizationNumber, ex.Message);
            }

            var orgNumbers = brResults.Select(br => br.OrganizationNumber).ToHashSet();
            result.AuditCoordinations =
                result.AuditCoordinations?.Where(r => orgNumbers.Contains(r.ControlObject)).ToList();
        }
        // If brResults is null or empty, filtered will be the same as it was before
        var filtered = (AuditCoordinationList)filterService.FilterAuditList(result, brResults);
        ecb.AddEvidenceValue($"tilsynskoordineringer", JsonConvert.SerializeObject(filtered, Formatting.None), result.ControlAgency, false);

        return ecb.GetEvidenceValues();
    }

    private List<Task<AuditCoordinationList>> GetReportListTasks(EvidenceHarvesterRequest req, TildaParameters param, bool npdid)
    {
        var taskList = new List<Task<AuditCoordinationList>>();
        try
        {
            if (npdid)
            {
                foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaNPDIDAuditCoordination>(param.sourceFilter))
                {
                    taskList.Add(a.GetAuditCoordinationAsync(req, param.fromDate, param.toDate));
                }
            }
            else
            {
                foreach (var a in tildaSourceProvider.GetRelevantSources<ITildaAuditCoordination>(param.sourceFilter))
                {
                    taskList.Add(a.GetAuditCoordinationAsync(req, param.fromDate, param.toDate));
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
