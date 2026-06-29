using Altinn.ApiClients.Maskinporten.Interfaces;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;
using System.Net.Http;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class TestTilsyn : TildaDataSource, ITildaAlertMessage
    {
        private const string orgNo = "111111111";
        private const string controlAgency = "TestTilsyn";

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public override bool TestOnly => true;
        private readonly string _code;

        public TestTilsyn(IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            ResiliencePipelineProvider<string> pipelineProvider,
            IUriFormatter uriFormatter,
            IMaskinportenService maskinportenService) :
            base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter, maskinportenService)
        {
            _code = _settings.GetClassBaseCode(GetType().Name);
        }

        public TestTilsyn() : base()
        {

        }

        protected override string GetAlertUri(string from) => $"{BaseUri}/{MtamDatasetName}?fromDate={from}&code={_code}";
        protected override string GetSingleAlertUri(string id, string requestor) =>
            $"{BaseUri}/{MtamDatasetName}/{id}?requestor={requestor}&code={_code}";

        protected override string PostAlertUri() => $"{BaseUri}/{MtamDatasetName}?code={_code}";
    }
}
