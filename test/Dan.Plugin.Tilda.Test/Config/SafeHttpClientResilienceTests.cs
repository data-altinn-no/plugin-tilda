using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Test.TestSupport;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Enums;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.Test.Config;

/// <summary>
/// Exercises the real SafeHttpClient pipeline (per-host breaker with an inner attempt timeout)
/// end to end through GetData, against an upstream that never answers.
/// </summary>
public class SafeHttpClientResilienceTests
{
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromMilliseconds(200);
    private readonly ILogger _logger = A.Fake<ILogger>();

    private static (HttpClient client, DelayingHttpMessageHandler handler) CreateClient()
    {
        var handler = new DelayingHttpMessageHandler(TimeSpan.FromSeconds(30));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient("SafeHttpClient", c => c.Timeout = TimeSpan.FromSeconds(10)) // outer backstop, like SafeHttpClientTimeout
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddPerHostResilience(AttemptTimeout);
        var client = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("SafeHttpClient");
        return (client, handler);
    }

    [Fact]
    public async Task SlowUpstream_FailsAtAttemptTimeout_NotAtHttpClientTimeout()
    {
        var (client, _) = CreateClient();
        var sw = Stopwatch.StartNew();

        var result = await client.GetData<AuditReportList>("https://a.test/x", "1", _logger, "t", "r");

        sw.Stop();
        result.Status.Should().Be(StatusEnum.Failed);
        result.StatusText.Should().Contain("timeout");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task RepeatedTimeouts_OpenCircuitPerAuthority()
    {
        var (client, handler) = CreateClient();

        // MinimumThroughput is 8: eight timeouts inside the 30s sampling window open the breaker.
        for (var i = 0; i < 8; i++)
        {
            var r = await client.GetData<AuditReportList>("https://a.test/x", "1", _logger, "t", "r");
            r.Status.Should().Be(StatusEnum.Failed);
        }
        var callsBeforeOpen = handler.Calls;

        var sw = Stopwatch.StartNew();
        var rejected = await client.GetData<AuditReportList>("https://a.test/x", "1", _logger, "t", "r");
        sw.Stop();

        rejected.Status.Should().Be(StatusEnum.Failed);
        rejected.StatusText.Should().Contain("circuit");
        sw.Elapsed.Should().BeLessThan(AttemptTimeout, "an open breaker must fail fast without waiting for the attempt timeout");
        handler.Calls.Should().Be(callsBeforeOpen, "an open breaker must not reach the upstream");

        // A different authority has its own breaker state: it still gets a real (timed-out) attempt.
        var other = await client.GetData<AuditReportList>("https://b.test/x", "2", _logger, "t", "r");
        other.Status.Should().Be(StatusEnum.Failed);
        other.StatusText.Should().Contain("timeout");
        handler.Calls.Should().Be(callsBeforeOpen + 1);
    }
}
