using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Config;

namespace Dan.Plugin.Tilda.Test.Config;

public class SettingsTests
{
    private sealed class CountingSecretStore : ISecretStore
    {
        private static readonly X509Certificate2 Certificate = CreateSelfSigned();
        private int _secretCalls;
        private int _certificateCalls;

        public int SecretCalls => _secretCalls;
        public int CertificateCalls => _certificateCalls;
        public Func<string, Task<string>> OnSecret { get; set; } = _ => Task.FromResult(string.Empty);

        public async Task<string> GetSecretAsync(string name)
        {
            Interlocked.Increment(ref _secretCalls);
            await Task.Delay(30);
            return await OnSecret(name);
        }

        public async Task<X509Certificate2> GetCertificateAsync(string name)
        {
            Interlocked.Increment(ref _certificateCalls);
            await Task.Delay(30);
            return Certificate;
        }

        private static X509Certificate2 CreateSelfSigned()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=settings-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }
    }

    [Fact]
    public async Task GetDigdirCertificateAsync_ConcurrentFirstCalls_LoadOnce()
    {
        // A cold instance hit by a burst must not stampede Key Vault: one load, shared by all callers.
        var store = new CountingSecretStore();
        var settings = new Settings { DigdirCertificateName = "digdir-cert", SecretStore = store };

        var results = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => settings.GetDigdirCertificateAsync()));

        store.CertificateCalls.Should().Be(1);
        results.Should().OnlyContain(c => ReferenceEquals(c, results[0]));
    }

    [Fact]
    public async Task GetTildaP6Async_FirstLoadFails_NextCallRetries()
    {
        var store = new CountingSecretStore();
        var attempt = 0;
        store.OnSecret = _ => ++attempt == 1
            ? Task.FromException<string>(new InvalidOperationException("key vault hiccup"))
            : Task.FromResult("1, 2,3");
        var settings = new Settings { SecretStore = store };

        var first = async () => await settings.GetTildaP6Async();
        await first.Should().ThrowAsync<InvalidOperationException>();

        var list = await settings.GetTildaP6Async();

        list.Should().Equal("1", "2", "3");
        store.SecretCalls.Should().Be(2);

        // Cached after a successful load.
        await settings.GetTildaP6Async();
        store.SecretCalls.Should().Be(2);
    }

    [Fact]
    public async Task SecretStore_NotConfigured_Throws()
    {
        var settings = new Settings();

        var act = async () => await settings.GetTildaP9Async();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
