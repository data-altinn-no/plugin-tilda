using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Utils;
using Dan.Tilda.Models.Audits.Storulykke;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Functions;

public class StorulykkeVirksomhetFunctions(IEvidenceSourceMetadata metadata)
{
    private List<string> p6Orgs;
    private List<string> p9Orgs;

    [Function("TildaStorulykkevirksomhet")]
    public async Task<HttpResponseData> TildaStorulykkevirksomhet([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaStorulykkevirksomhet")] HttpRequestData req, FunctionContext context)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        await GetStorulykkeProps();

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesStorulykkevirksomhet(evidenceHarvesterRequest));
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // EvidenceSourceResponse.CreateResponse doesn't support methods that don't return a Task. Need to update Common with that functionality first
    private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhet(EvidenceHarvesterRequest evidenceHarvesterRequest)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var eb = new EvidenceBuilder(metadata, "TildaStorulykkevirksomhet");

        var result = new StorulykkevirksomhetKontroll
        {
            OrganizationNumber = evidenceHarvesterRequest.OrganizationNumber
        };

        if (p6Orgs.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph6 = true;
        }

        if (p9Orgs.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph9 = true;

        }

        eb.AddEvidenceValue("Storulykkevirksomhet", JsonConvert.SerializeObject(result), "Tilda", false);

        return eb.GetEvidenceValues();
    }

    private async Task GetStorulykkeProps()
    {
        p6Orgs = await ResourceManager.GetParagraph("6");
        p9Orgs = await ResourceManager.GetParagraph("9");
    }
}
