using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Services;
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
using Settings = Dan.Plugin.Tilda.Config.Settings;

var host = new HostBuilder()
    .ConfigureDanPluginDefaults()
   /* .ConfigureAppConfiguration((context, configuration) =>
    {
        configuration
            .AddConfiguration()

    }) */
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

        var distributedCache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

        var registry = new PolicyRegistry()
        {
            { "defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, TimeSpan.Parse(settings.Breaker_RetryWaitTime)) },
            { "ERCachePolicy", Policy.CacheAsync(distributedCache.AsAsyncCacheProvider<string>(), TimeSpan.FromHours(12)) }
        };

        services.AddPolicyRegistry(registry);

        services.AddHttpClient("SafeHttpClient", client =>
            {
                client.Timeout = new TimeSpan(0, 0, 30);
            })
            .AddPolicyHandlerFromRegistry("defaultCircuitBreaker");

        // Client configured without circuit breaker policies. shorter timeout
        services.AddHttpClient("ERHttpClient", client =>
        {
            client.Timeout = new TimeSpan(0, 0, 5);
        });


    })
    .Build();

await host.RunAsync();
