using System;
using System.Security.Cryptography.X509Certificates;

namespace Dan.Plugin.Tilda.Config
{
    public class Settings
    {
        public string GetClassBaseUri(string className)
        {
            return Environment.GetEnvironmentVariable(className + ".uri");
        }

        public string RedisConnectionString { get; set; }

        public bool IsTest { get; set; }

        public bool IsLocalDevelopment { get; set; }

        public string Breaker_RetryWaitTime { get; set; }
        public string Breaker_OpenCircuitTime { get; set; }

        public string KofuviEndpoint { get; set; }
        public string KvName { get; set; }
        public string KvKofuviCertificateName { get; set; }

        public string DataAltinnNoBaseUrl { get; set; }
        public string AltinnEventsBaseUrl { get; set; }

        public static string CosmosDbConnection => Environment.GetEnvironmentVariable("CosmosDbConnection");
        public string CosmosDbDatabase { get; set; }

        private static string KeyVaultName => Environment.GetEnvironmentVariable("KvName");
        private static string KofuviCertificateName => Environment.GetEnvironmentVariable("KvKofuviCertificateName");

        private static X509Certificate2 _altinnCertificate { get; set; }
        public static X509Certificate2 Certificate
        {
            get
            {
                return _altinnCertificate ?? new KeyVault(KeyVaultName).GetCertificate(KofuviCertificateName).Result;
            }
            set
            {
                _altinnCertificate = value;
            }
        }
    }
}
