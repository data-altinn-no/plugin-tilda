using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Dan.Plugin.Tilda.Config;

public static class LoggingFilters
{
    /// <summary>
    /// Categories that log several Information lines per outbound HTTP call. In production they
    /// were ~60% of all trace volume (System.Net.Http.HttpClient: 4 lines per request, Polly:
    /// "Resilience pipeline executed" per attempt) while carrying nothing the dependency
    /// telemetry does not already record.
    /// </summary>
    public static readonly (string Category, LogLevel MinimumLevel)[] NoisyCategories =
    [
        ("System.Net.Http.HttpClient", LogLevel.Warning),
        ("Polly", LogLevel.Warning),
        ("Microsoft", LogLevel.Warning),
    ];

    /// <summary>
    /// Applies the log filters for the worker.
    ///
    /// worker-logging.json is loaded into configuration, but a bare HostBuilder never binds its
    /// "Logging" section to the logger factory, so its LogLevel rules had no effect. Bind it here,
    /// and additionally pin the noisiest categories both provider-agnostically and specifically
    /// for the Application Insights provider: the filter algorithm ignores provider-agnostic rules
    /// for a provider whenever any provider-specific rule exists for it, which the AI SDK sets up.
    /// </summary>
    public static ILoggingBuilder Apply(this ILoggingBuilder logging, IConfiguration configuration)
    {
        logging.AddConfiguration(configuration.GetSection("Logging"));

        foreach (var (category, minimumLevel) in NoisyCategories)
        {
            logging.AddFilter(category, minimumLevel);
            logging.AddFilter<ApplicationInsightsLoggerProvider>(category, minimumLevel);
        }

        return logging;
    }
}
