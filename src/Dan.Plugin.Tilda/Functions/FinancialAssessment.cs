using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Tilda.Functions;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Dan.Plugin.TIlda.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dan.Plugin.Tilda.Functions
{
    public class FinancialAssessment        
    {
        private readonly IBrregService bbrreg;
        private ILogger logger;
        private readonly ITildaSourceProvider tildaSource;
        private readonly IEvidenceSourceMetadata metadata;

        public FinancialAssessment(IBrregService brregService, ILoggerFactory loggerFactory, IEvidenceSourceMetadata metadata) 
            {
            bbrreg = brregService;           
            logger = loggerFactory.CreateLogger<FinancialAssessment>();
            this.metadata = metadata;
        }

        [Function("TildaOkonomiskVurderingV1")]
        public async Task<HttpResponseData> TildaPdfReport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TildaOkonomiskVurderingV1")] HttpRequestData req, FunctionContext context)
        {         
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetFinancialAssessment(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber));
        }

        private async Task<List<EvidenceValue>> GetFinancialAssessment(string norwegianOrganizationNumber)
        {

            var accounts = await bbrreg.GetAnnualAccountsFromBr(norwegianOrganizationNumber);
            var unitInfo = await bbrreg.GetFromBr(norwegianOrganizationNumber, false, false);
            var assessor = new EconomicAssessment(accounts, unitInfo.First());

            var evalauation = assessor.Evaluate();

            var ecb = new EvidenceBuilder(metadata, "TildaOkonomiskVurderingV1");
            ecb.AddEvidenceValue("default", JsonConvert.SerializeObject(evalauation), "Regnskapsregisteret", false);

            return ecb.GetEvidenceValues();
        }

        private IEnumerable<AccountsInformationYear> GetAnnualAccounts()
        {
            //get last three years of annual accounts for the company, and return them as a list of AccountsInformationYear objects
            throw new NotImplementedException();
        }
    }
}



