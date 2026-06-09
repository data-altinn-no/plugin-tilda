using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dan.Common.Exceptions;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Services;
using FakeItEasy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Test.Services;

public class BrregServiceTests
{
    // Not one of the hardcoded TEST orgs (111111111 / 811105562) that GetFromBr short-circuits.
    private const string OrgNo = "999999999";

    private readonly IDistributedCache _cache = A.Fake<IDistributedCache>();
    private readonly IOptions<Settings> _settings = A.Fake<IOptions<Settings>>();
    private readonly ILogger<BrregService> _logger = A.Fake<ILogger<BrregService>>();

    public BrregServiceTests()
    {
        A.CallTo(() => _settings.Value).Returns(new Settings { KofuviEndpoint = "https://kofuvi.test" });

        // Always a cache miss, so every call exercises the (stubbed) HTTP layer.
        A.CallTo(() => _cache.GetAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<byte[]?>(null));
    }

    private BrregService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var client = new HttpClient(new StubHttpMessageHandler(responder));
        var factory = A.Fake<IHttpClientFactory>();
        // BrregService builds SafeHttpClient/ERHttpClient/KofuviClient; each test exercises one.
        A.CallTo(() => factory.CreateClient(A<string>._)).Returns(client);
        return new BrregService(factory, _cache, _settings, _logger);
    }

    // --- GetKofuviAddresses: soft + hard fails both degrade gracefully to an empty list ---

    [Fact]
    public async Task GetKofuviAddresses_SoftFail_NonSuccessStatus_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var result = await sut.GetKofuviAddresses(OrgNo);

        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetKofuviAddresses_SoftFail_NotFound_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await sut.GetKofuviAddresses(OrgNo);

        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetKofuviAddresses_HardFail_RequestException_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => throw new HttpRequestException("connection refused"));

        var result = await sut.GetKofuviAddresses(OrgNo);

        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetKofuviAddresses_HardFail_Timeout_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => throw new TaskCanceledException("timeout", new TimeoutException()));

        var result = await sut.GetKofuviAddresses(OrgNo);

        result.Should().NotBeNull().And.BeEmpty();
    }

    // --- GetAnnualTurnoverFromBr: soft + hard fails both degrade to an empty AccountsInformation
    // (no throw, no turnover data). ---

    [Fact]
    public async Task GetAnnualTurnoverFromBr_SoftFail_NonSuccessStatus_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await sut.GetAnnualTurnoverFromBr(OrgNo);

        result.Should().NotBeNull();
        result.AnnualTurnover.Should().BeNull();
    }

    [Fact]
    public async Task GetAnnualTurnoverFromBr_HardFail_Timeout_ReturnsEmptyWithoutThrowing()
    {
        var sut = CreateService(_ => throw new TaskCanceledException("timeout", new TimeoutException()));

        var result = await sut.GetAnnualTurnoverFromBr(OrgNo);

        result.Should().NotBeNull();
        result.AnnualTurnover.Should().BeNull();
    }

    // --- GetFromBr / GetOrganizationInfoFromBr: fails surface as exceptions for the caller ---

    [Fact]
    public async Task GetFromBr_SoftFail_NotFound_ThrowsPermanentClientException()
    {
        // 404 on both main unit and sub unit => organization genuinely not found.
        var sut = CreateService(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var act = async () => await sut.GetFromBr(OrgNo, includeSubunits: false);

        await act.Should().ThrowAsync<EvidenceSourcePermanentClientException>();
    }

    [Fact]
    public async Task GetFromBr_HardFail_RequestException_ThrowsPermanentServerException()
    {
        var sut = CreateService(_ => throw new HttpRequestException("dns failure"));

        var act = async () => await sut.GetFromBr(OrgNo, includeSubunits: false);

        await act.Should().ThrowAsync<EvidenceSourcePermanentServerException>();
    }

    [Fact]
    public async Task GetFromBr_HardFail_Timeout_PropagatesToCaller()
    {
        // Contract: BrregService does NOT translate HttpClient timeouts. The caller
        // (AuditFunctionsBase.GetOrganizationsFromBr) catches them and fails closed, so the
        // exception must escape here rather than being swallowed into a "found nothing" result.
        var sut = CreateService(_ => throw new TaskCanceledException("timeout", new TimeoutException()));

        var act = async () => await sut.GetFromBr(OrgNo, includeSubunits: false);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(responder(request));
            }
            catch (Exception ex)
            {
                return Task.FromException<HttpResponseMessage>(ex);
            }
        }
    }
}
