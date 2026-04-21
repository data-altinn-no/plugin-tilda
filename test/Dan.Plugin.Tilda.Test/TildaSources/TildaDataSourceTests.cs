using System;
using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.TildaSources;
using Dan.Plugin.Tilda.Utils;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Dan.Plugin.Tilda.Test.TildaSources;

public class TildaDataSourceTests
{
    private readonly IOptions<Settings> settings = A.Fake<IOptions<Settings>>();
    private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
    private readonly ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
    private readonly ResiliencePipelineProvider<string> pipelineProvider = A.Fake<ResiliencePipelineProvider<string>>();
    private readonly IUriFormatter uriFormatter = A.Fake<IUriFormatter>();
    private readonly IMaskinportenService maskinportenService = A.Fake<IMaskinportenService>();

    [Theory]
    [InlineData("https://test.test.test/", "https://test.test.test")]
    [InlineData("https://test.test.test", "https://test.test.test")]
    [InlineData(null, null)]
    public void Constructor_BaseUrl_ShouldRemoveTrailingSlash(string? inputUrl, string? expected)
    {
        // Arrange
        Environment.SetEnvironmentVariable("TildaDataSourceDummy.uri", inputUrl);
        var sett = new Settings();
        A.CallTo(() => settings.Value).Returns(sett);

        // Act
        var tildaDataSource = new TildaDataSourceDummy(settings, httpClientFactory,loggerFactory, pipelineProvider, uriFormatter, maskinportenService);

        // Assert
        tildaDataSource.BaseUri.Should().Be(expected);
    }
}

internal class TildaDataSourceDummy : TildaDataSource
{
    public TildaDataSourceDummy(IOptions<Settings> settings,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        ResiliencePipelineProvider<string> pipelineProvider,
        IUriFormatter uriFormatter,
        IMaskinportenService maskinportenService) :
        base(settings, httpClientFactory, loggerFactory, pipelineProvider, uriFormatter, maskinportenService)
    {

    }

    public override string OrganizationNumber { get; }
    public override string ControlAgency { get; }
}
