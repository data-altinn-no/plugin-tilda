using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class PdfFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata) : AuditFunctionsBase(brregService)
{
    private ILogger logger;

    [Function("TildaTilsynsrapportpdfv1")]
    public async Task<HttpResponseData> TildaPdfReport(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaTilsynsrapportpdfv1")] HttpRequestData req, FunctionContext context)
    {
        logger = context.GetLogger(context.FunctionDefinition.Name);
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesTildaPdfReportV1(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTildaPdfReportV1(EvidenceHarvesterRequest req)
    {
        if (!req.TryGetParameter("internTilsynsId", out string id))
        {
            throw new EvidenceSourcePermanentClientException(1, $"Missing required parameter internTilsynsId");
        }

        var filter = req.SubjectParty?.NorwegianOrganizationNumber;

        try
        {
            //Should always only return ONE source
            var pdfTarget = tildaSourceProvider.GetRelevantSources<ITildaPdfReport>(filter).FirstOrDefault();

            if (pdfTarget == null)
            {
                logger.LogError("plugin tilda does not support pdf for source {filter}", filter);
                throw new EvidenceSourcePermanentClientException(1, $"Source {filter} does not support pdf reports");
            }

            var result = await pdfTarget.GetPdfReport(req, id);
            var ecb = new EvidenceBuilder(metadata, "TildaTilsynsrapportpdfv1");
            ecb.AddEvidenceValue($"pdfrapport", result, "Tilda", false);

            return ecb.GetEvidenceValues();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new EvidenceSourcePermanentClientException(1, $"Source {filter} does not support pdf reports");
        }
    }
}
