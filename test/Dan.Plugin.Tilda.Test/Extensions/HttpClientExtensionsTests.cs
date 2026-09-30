using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Test.TestSupport;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Enums;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Dan.Plugin.Tilda.Test.Extensions;

public class HttpClientExtensionsTests
{
    private const string SourceOrgNo = "874761192";
    private const string Requestor = "999601391";
    private const string Url = "https://source.test/tilda/tilsyn/999999999";
    private readonly ILogger _logger = A.Fake<ILogger>();

    private static HttpClient ClientThatThrows(Exception ex) =>
        new(new StubHttpMessageHandler(_ => throw ex));

    [Fact]
    public async Task GetData_TimeoutRejected_ReturnsFailedWithTimeoutText()
    {
        // The per-attempt timeout inside the resilience pipeline surfaces as TimeoutRejectedException.
        var client = ClientThatThrows(new TimeoutRejectedException("attempt timeout"));

        var result = await client.GetData<AuditReportList>(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Status.Should().Be(StatusEnum.Failed);
        result.StatusText.Should().Contain("timeout");
        result.GetOwner().Should().Be(SourceOrgNo);
    }

    [Fact]
    public async Task GetData_BrokenCircuit_ReturnsFailedWithCircuitText()
    {
        var client = ClientThatThrows(new BrokenCircuitException("open"));

        var result = await client.GetData<AuditReportList>(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Status.Should().Be(StatusEnum.Failed);
        result.StatusText.Should().Contain("circuit");
        result.GetOwner().Should().Be(SourceOrgNo);
    }

    [Fact]
    public async Task GetData_HttpClientTimeout_ReturnsFailed()
    {
        // Outer HttpClient.Timeout backstop (SafeHttpClientTimeout) still maps to Failed.
        var client = ClientThatThrows(new TaskCanceledException("timeout", new TimeoutException()));

        var result = await client.GetData<AuditReportList>(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Status.Should().Be(StatusEnum.Failed);
        result.StatusText.Should().Contain("timeout");
    }

    [Fact]
    public async Task GetData_Ok_ReturnsOk()
    {
        var client = new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":0}")
        }));

        var result = await client.GetData<AuditReportList>(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Status.Should().Be(StatusEnum.Ok);
        result.GetOwner().Should().Be(SourceOrgNo);
    }

    [Fact]
    public async Task GetPdfreport_BrokenCircuit_ReturnsNullWithoutThrowing()
    {
        var client = ClientThatThrows(new BrokenCircuitException("open"));

        var result = await client.GetPdfreport(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPdfreport_TimeoutRejected_ReturnsNullWithoutThrowing()
    {
        var client = ClientThatThrows(new TimeoutRejectedException("attempt timeout"));

        var result = await client.GetPdfreport(Url, SourceOrgNo, _logger, "token", Requestor);

        result.Should().BeNull();
    }
}
