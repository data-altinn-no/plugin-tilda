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
using Azure.Core;
using Azure.Identity;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Mappers;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.Utils;
using Microsoft.Azure.Cosmos.Fluent;
using Polly.Retry;
using StackExchange.Redis;
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

        DefaultAzureCredential credentials = new();
        services.AddSingleton(credentials);
        // In case of still using access key (or local redis),
        if (settings.RedisConnectionString.Contains("password=") ||
            settings.RedisConnectionString.Contains("127.0.0.1"))
        {
            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = settings.RedisConnectionString;
            });
        }
        else
        {
            services.AddStackExchangeRedisCache(option =>
            {
                option.ConnectionMultiplexerFactory = async () =>
                {
                    var configurationOptions = await ConfigurationOptions
                        .Parse(settings.RedisConnectionString)
                        .ConfigureForAzureWithTokenCredentialAsync(credentials);

                    var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

                    return connectionMultiplexer;
                };
            });
        }

        if (Settings.CosmosDbConnection.StartsWith("AccountEndpoint="))
        {
            services.AddSingleton(_ => new CosmosClientBuilder(Settings.CosmosDbConnection).Build());
        }
        else
        {
            services.AddSingleton(_ => new CosmosClientBuilder(Settings.CosmosDbConnection, credentials).Build());

        }
        services.AddSingleton<IEntityRegistryService, EntityRegistryService>();
        services.AddSingleton<IEvidenceSourceMetadata, Metadata>();

        services.AddTransient<IMtamCounterClient, MtamCounterClient>();
        services.AddTransient<IAlertMessageSender, AlertMessageSender>();
        services.AddTransient<IAlertMessageMapper, AlertMessageMapper>();
        services.AddTransient<IEvidenceService, EvidenceService>();
        services.AddTransient<IBrregService, BrregService>();
        services.AddTransient<IFilterService, FilterService>();
        services.AddTransient<IUriFormatter, UriFormatter>();

        // Registers all implementations of ITildaDataSources under that interface, making it accessible with
        // dependency injection by injecting IEnumerable<ITildaDataSource> in constructor
        Assembly.GetAssembly(typeof(Program))!
            .ExportedTypes
            .Where(type =>
                type is { IsClass: true, Namespace: "Dan.Plugin.Tilda.TildaSources", IsNestedPrivate: false } &&
                typeof(ITildaDataSource).IsAssignableFrom(type))
            .ToList()
            .ForEach(type => services.AddTransient(typeof(ITildaDataSource), type));

        services.AddSingleton<ITildaSourceProvider, TildaSourceProvider>();

        var policyRegistry = services.BuildServiceProvider().GetRequiredService<IPolicyRegistry<string>>();
        policyRegistry.Add("defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, TimeSpan.Parse(settings.Breaker_RetryWaitTime)));

        // Client configured without circuit breaker policies. shorter timeout
        services.AddHttpClient("ERHttpClient", client =>
        {
            client.Timeout = new TimeSpan(0, 0, 5);
        });

        services.AddHttpClient("KofuviClient", client =>
            {
                client.Timeout = new TimeSpan(0, 0, 5);
            })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(Settings.Certificate);
            return handler;
        });

        services.AddResiliencePipeline("alert-pipeline", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,  // Adds a random factor to the delay
                    MaxRetryAttempts = 6,
                    Delay = TimeSpan.FromSeconds(5),
                })
                .AddTimeout(TimeSpan.FromSeconds(120));
        });
    })
    .Build();

await host.RunAsync();
