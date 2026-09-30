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

    private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhet(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var eb = new EvidenceBuilder(_metadata, "TildaStorulykkevirksomhet");

        var p6 = await _settings.GetTildaP6Async();
        var p9 = await _settings.GetTildaP9Async();

        var result = new StorulykkevirksomhetKontroll
        {
            OrganizationNumber = evidenceHarvesterRequest.OrganizationNumber
        };

        if (p6.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph6 = true;
        }

        if (p9.Contains(evidenceHarvesterRequest.OrganizationNumber))
        {
            result.Paragraph9 = true;
        }

        eb.AddEvidenceValue("Storulykkevirksomhet", JsonConvert.SerializeObject(result), "Tilda", false);

        return eb.GetEvidenceValues();
    }

    [Function("TildaStorulykkevirksomhetAlle")]
    public async Task<HttpResponseData> TildaStorulykkevirksomhetAlle([HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaStorulykkevirksomhetAlle")] HttpRequestData req, FunctionContext context)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesStorulykkevirksomhetAlle(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesStorulykkevirksomhetAlle(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var eb = new EvidenceBuilder(_metadata, "TildaStorulykkevirksomhetAlle");

        var p6 = await _settings.GetTildaP6Async();
        var p9 = await _settings.GetTildaP9Async();

        eb.AddEvidenceValue("StorulykkevirksomheterParagraf6", JsonConvert.SerializeObject(new StorulykkevirksomhetListe() { Organizations = p6 }), "Tilda", false);
        eb.AddEvidenceValue("StorulykkevirksomheterParagraf9", JsonConvert.SerializeObject(new StorulykkevirksomhetListe() { Organizations = p9 }), "Tilda", false);

        return eb.GetEvidenceValues();
    }
}
