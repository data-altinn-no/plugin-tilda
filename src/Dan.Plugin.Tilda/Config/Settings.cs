using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Config
{
    public class Settings
    {
        public string GetClassBaseUri(string className)
        {
            return Environment.GetEnvironmentVariable(className + ".uri");
        }

        public string GetClassBaseCode(string className)
        {
            return Environment.GetEnvironmentVariable(className + ".code");
        }

        public string RedisConnectionString { get; set; }

        public bool IsTest { get; set; }

        public bool IsLocalDevelopment { get; set; }

        public string KofuviEndpoint { get; set; }
        public string KvName { get; set; }
        public string KvKofuviCertificateName { get; set; }

        public string DataAltinnNoBaseUrl { get; set; }

        public static string CosmosDbConnection => Environment.GetEnvironmentVariable("CosmosDbConnection");
        public string CosmosDbDatabase { get; set; }

        /// <summary>
        /// Per-attempt timeout applied inside the SafeHttpClient resilience pipeline (see
        /// SafeHttpClientResilience). Must be strictly shorter than SafeHttpClientTimeout
        /// (the outer HttpClient.Timeout applied by Dan.Common), which is validated at startup.
        /// </summary>
        public int SafeHttpClientAttemptTimeoutSeconds { get; set; } = 5;

        public string DigdirCertificateName { get; set; }

        public string MaskinportenEnvironment { get; set; }
        public string ClientId { get; set; }

        /// <summary>
        /// Secret store used to lazily load certificates and lists. Internal (not bound from
        /// configuration); set via PostConfigure in Program.cs, or directly by tests.
        /// </summary>
        internal ISecretStore SecretStore { get; set; }

        private readonly CachedSecret<X509Certificate2> _kofuviCertificate = new();
        private readonly CachedSecret<X509Certificate2> _digdirCertificate = new();
        private readonly CachedSecret<IReadOnlyList<string>> _tildaP6 = new();
        private readonly CachedSecret<IReadOnlyList<string>> _tildaP9 = new();

        public Task<X509Certificate2> GetKofuviCertificateAsync() =>
            _kofuviCertificate.GetAsync(() => Store().GetCertificateAsync(KvKofuviCertificateName));

        public Task<X509Certificate2> GetDigdirCertificateAsync() =>
            _digdirCertificate.GetAsync(() => Store().GetCertificateAsync(DigdirCertificateName));

        public Task<IReadOnlyList<string>> GetTildaP6Async() =>
            _tildaP6.GetAsync(async () => SplitList(await Store().GetSecretAsync("TildaP6")));

        public Task<IReadOnlyList<string>> GetTildaP9Async() =>
            _tildaP9.GetAsync(async () => SplitList(await Store().GetSecretAsync("TildaP9")));

        /// <summary>Pre-seed the Digdir certificate (tests / local development without Key Vault).</summary>
        internal void SetDigdirCertificate(X509Certificate2 certificate) => _digdirCertificate.Set(certificate);

        private ISecretStore Store() =>
            SecretStore ?? throw new InvalidOperationException("Settings.SecretStore is not configured");

        private static IReadOnlyList<string> SplitList(string csv) =>
            (csv ?? string.Empty).Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}
