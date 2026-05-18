using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Config;
using Dan.Tilda.Models.Audits.Storulykke;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Functions;

public class StorulykkeVirksomhetFunctions
{

    private readonly Settings _settings;
    private readonly IEvidenceSourceMetadata _metadata;

    public StorulykkeVirksomhetFunctions(IOptions<Settings> settings, IEvidenceSourceMetadata metadata)
    {
        _settings = settings.Value;
        _metadata = metadata;
    }

    [Function("TildaStorulykkevirksomhet")]
    public async Task<HttpResponseData> TildaStorulykkevirksomhet([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaStorulykkevirksomhet")] HttpRequestData req, FunctionContext context)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesStorulykkevirksomhet(evidenceHarvesterRequest));
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // EvidenceSourceResponse.CreateResponse doesn't support methods that don't return a Task. Need to update Common with that functionality first
    private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhet(EvidenceHarvesterRequest evidenceHarvesterRequest)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var eb = new EvidenceBuilder(_metadata, "TildaStorulykkevirksomhet");

        var a = _settings.TildaP6;

        var b = _settings.TildaP9;


        var result = new StorulykkevirksomhetKontroll
        {
            OrganizationNumber = evidenceHarvesterRequest.OrganizationNumber
        };

        if (_settings.TildaP6.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph6 = true;
        }

        if (_settings.TildaP9.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph9 = true;

        }

        eb.AddEvidenceValue("Storulykkevirksomhet", JsonConvert.SerializeObject(result), "Tilda", false);

        return eb.GetEvidenceValues();
    }
}
