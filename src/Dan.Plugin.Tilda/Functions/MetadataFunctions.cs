using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dan.Plugin.Tilda.Functions;

public class MetadataFunctions(
    IBrregService brregService,
    ITildaSourceProvider tildaSourceProvider,
    IEvidenceSourceMetadata metadata) : AuditFunctionsBase(brregService)
{

    [Function("TildaMetadatav1")]
    public async Task<HttpResponseData> TildaMetadata(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaMetadatav1")] HttpRequestData req, FunctionContext context)
    {
        return await EvidenceSourceResponse.CreateResponse(req, GetEvidenceValuesTildaMetadata);
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesTildaMetadata()
    {
        var ecb = new EvidenceBuilder(metadata, "TildaMetadatav1");

        var trend = tildaSourceProvider.GetAllSources<ITildaTrendReports>().Select(x=>x.OrganizationNumber + ":" + x.ControlAgency);
        var trendAll = tildaSourceProvider.GetAllSources<ITildaTrendReportsAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var audit = tildaSourceProvider.GetAllSources<ITildaAuditReports>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
        var auditAll = tildaSourceProvider.GetAllSources<ITildaAuditReportsAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var coordination = tildaSourceProvider.GetAllSources<ITildaAuditCoordination>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);
        var coordinationAll = tildaSourceProvider.GetAllSources<ITildaAuditCoordinationAll>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var npdid = tildaSourceProvider.GetAllSources<ITildaNPDIDAuditReports>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var pdfReport = tildaSourceProvider.GetAllSources<ITildaPdfReport>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var alertMessages = tildaSourceProvider.GetAllSources<ITildaAlertMessage>().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        var all = tildaSourceProvider.GetAllRegisteredSources().Select(x => x.OrganizationNumber + ":" + x.ControlAgency);

        ecb.AddEvidenceValue("TildaTrendrapportv1", string.Join(",", trend), "Tilda", false);
        ecb.AddEvidenceValue("TildaTrendrapportAllev1", string.Join(",", trendAll), "Tilda", false);

        ecb.AddEvidenceValue("TildaTilsynsrapportv1", string.Join(",", audit), "Tilda", false);
        ecb.AddEvidenceValue("TildaTilsynsrapportAllev1", string.Join(",", auditAll), "Tilda", false);

        ecb.AddEvidenceValue("TildaTilsynskoordineringv1", string.Join(",", coordination), "Tilda", false);
        ecb.AddEvidenceValue("TildaTilsynskoordineringAllev1", string.Join(",", coordinationAll), "Tilda", false);

        ecb.AddEvidenceValue("TildaNPDIDv1", string.Join(",", npdid), "Tilda", false);

        ecb.AddEvidenceValue("TildaTilsynsrapportpdfv1", string.Join(",", pdfReport), "Tilda", false);

        ecb.AddEvidenceValue("TildaMeldingTilAnnenMyndighetv1", string.Join(",", alertMessages), "Tilda", false);

        ecb.AddEvidenceValue("AlleKilder", string.Join(",", all), "Tilda", false);

        return await Task.FromResult(ecb.GetEvidenceValues());
    }
}
