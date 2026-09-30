using System;
using System.Collections.Generic;
using Dan.Plugin.Tilda.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Dan.Plugin.Tilda.Test.Config;

public class LoggingFiltersTests
{
    private static ILoggerFactory CreateFactory(IReadOnlyDictionary<string, string?> loggingConfig)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(loggingConfig).Build();
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.AddProvider(new CapturingLoggerProvider());
            logging.Apply(configuration);
        });
        return services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    }

    private static readonly Dictionary<string, string?> WorkerLoggingJson = new()
    {
        ["Logging:LogLevel:Default"] = "Information",
        ["Logging:LogLevel:Dan.Plugin.Tilda"] = "Information",
        ["Logging:LogLevel:System.Net"] = "Warning",
    };

    [Theory]
    [InlineData("System.Net.Http.HttpClient.SafeHttpClient.LogicalHandler")]
    [InlineData("System.Net.Http.HttpClient.ERHttpClient.ClientHandler")]
    [InlineData("Polly")]
    [InlineData("Microsoft.Hosting.Lifetime")]
    public void NoisyCategories_InformationIsFiltered_WarningPasses(string category)
    {
        var logger = CreateFactory(WorkerLoggingJson).CreateLogger(category);

        logger.IsEnabled(LogLevel.Information).Should().BeFalse();
        logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
    }

    [Fact]
    public void PluginCategories_InformationStillPasses_DebugDoesNot()
    {
        var logger = CreateFactory(WorkerLoggingJson).CreateLogger("Dan.Plugin.Tilda.TildaSources.TildaDataSource");

        logger.IsEnabled(LogLevel.Information).Should().BeTrue();
        logger.IsEnabled(LogLevel.Debug).Should().BeFalse("the per-source started/completed lines are Debug and must not be emitted");
    }

    [Fact]
    public void ConfigurationSection_IsBound()
    {
        // A rule that exists only in the Logging section must take effect, proving the section is bound.
        var config = new Dictionary<string, string?>(WorkerLoggingJson) { ["Logging:LogLevel:Some.Custom.Category"] = "Error" };
        var logger = CreateFactory(config).CreateLogger("Some.Custom.Category.Child");

        logger.IsEnabled(LogLevel.Warning).Should().BeFalse();
        logger.IsEnabled(LogLevel.Error).Should().BeTrue();
    }

    [Fact]
    public void NoisyCategories_HaveProviderSpecificRulesForApplicationInsights()
    {
        // The AI SDK registers provider-specific rules, which make the logger ignore every
        // provider-agnostic rule for that provider. Our noisy-category rules must therefore also
        // exist as ApplicationInsightsLoggerProvider-specific rules.
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(WorkerLoggingJson).Build();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.Apply(configuration));
        var options = services.BuildServiceProvider()
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<LoggerFilterOptions>>().Value;

        foreach (var (category, level) in LoggingFilters.NoisyCategories)
        {
            options.Rules.Should().Contain(r =>
                r.ProviderName == typeof(ApplicationInsightsLoggerProvider).FullName &&
                r.CategoryName == category &&
                r.LogLevel == level);
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new NoopLogger();
        public void Dispose() { }

        private sealed class NoopLogger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}
