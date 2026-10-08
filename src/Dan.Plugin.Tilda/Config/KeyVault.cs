using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Dan.Plugin.Tilda.Config;

public interface ISecretStore
{
    /// <summary>Get a secret value by name.</summary>
    Task<string> GetSecretAsync(string name);

    /// <summary>Get a certificate stored as a base64 PKCS#12 secret.</summary>
    Task<X509Certificate2> GetCertificateAsync(string name);
}

/// <summary>
/// Key Vault-backed secret store. Takes the process-wide TokenCredential so it shares the
/// credential chain and token cache with everything else instead of probing a fresh
/// DefaultAzureCredential on every read.
/// </summary>
public sealed class KeyVault : ISecretStore
{
    private readonly SecretClient _client;

    public KeyVault(string vaultName, TokenCredential credential)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
        {
            throw new ArgumentException("Key Vault name is not configured (KvName)", nameof(vaultName));
        }

        _client = new SecretClient(new Uri($"https://{vaultName}.vault.azure.net/"), credential);
    }

    public async Task<string> GetSecretAsync(string name)
    {
        var secret = await _client.GetSecretAsync(name);
        return secret.Value.Value;
    }

    public async Task<X509Certificate2> GetCertificateAsync(string name)
    {
        var base64Certificate = await GetSecretAsync(name);
        var certBytes = Convert.FromBase64String(base64Certificate);
        return X509CertificateLoader.LoadPkcs12(certBytes, string.Empty, X509KeyStorageFlags.MachineKeySet);
    }
}
