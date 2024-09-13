using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Services;
using Dan.Plugin.Tilda;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching.Distributed;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Utils;
using Settings = Dan.Plugin.Tilda.Config.Settings;

var host = new HostBuilder()
    .ConfigureDanPluginDefaults()
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration
            .AddJsonFile("worker-logging.json", optional:true);
    })
    .ConfigureServices((context, services) =>
    {
        // This makes IOption<Settings> available in the DI container.
        var configurationRoot = context.Configuration;
        services.Configure<Settings>(configurationRoot);

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<Settings>>().Value;

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = settings.RedisConnectionString;
        });

        services.AddSingleton<IEntityRegistryService, EntityRegistryService>();
        services.AddSingleton<IEvidenceSourceMetadata, Metadata>();

        // Registers all implementations of ITildaDataSources under that interface, making it accessible with
        // dependcy injection by injecting IEnumerable<ITildaDataSource> in constructor
        Assembly.GetAssembly(typeof(Program))!
            .ExportedTypes
            .Where(type => type is { IsClass: true, Namespace: "Dan.Plugin.Tilda.TildaSources", IsNestedPrivate: false } && typeof(ITildaDataSource).IsAssignableFrom(type))
            .ToList()
            .ForEach(type => services.AddTransient(typeof(ITildaDataSource), type));

        services.AddSingleton<ISourceProvider, SourceProvider>();

        var distributedCache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

        var policyRegistry = services.BuildServiceProvider().GetRequiredService<IPolicyRegistry<string>>();
        policyRegistry.Add("defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, TimeSpan.Parse(settings.Breaker_RetryWaitTime)));
        policyRegistry.Add("ERCachePolicy", Policy.CacheAsync(distributedCache.AsAsyncCacheProvider<string>(), TimeSpan.FromHours(12)));

        // Client configured without circuit breaker policies. shorter timeout
        services.AddHttpClient("ERHttpClient", client =>
        {
            client.Timeout = new TimeSpan(0, 0, 5);
        });

        services.AddHttpClient("KofuviClient", _ =>
        {

        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(Settings.Certificate);
            return handler;
        });

    })
    .Build();

await host.RunAsync();
