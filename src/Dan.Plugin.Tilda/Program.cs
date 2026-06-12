using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Services;
using Dan.Plugin.Tilda;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Registry;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Azure.Identity;
using Dan.Plugin.Tilda.Clients;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Mappers;
using Dan.Plugin.Tilda.Services;
using Dan.Plugin.Tilda.TildaSources;
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

        // Bind a local copy for use during service registration, without building an
        // intermediate service provider (which would construct duplicate singletons)
        var settings = configurationRoot.Get<Settings>();

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
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(settings.RedisConnectionString));
        }
        else
        {
            var configurationOptions = ConfigurationOptions
                .Parse(settings.RedisConnectionString)
                .ConfigureForAzureWithTokenCredentialAsync(credentials)
                .GetAwaiter().GetResult();

            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(option =>
            {
                option.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
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
        services.AddSingleton<ITokenCacheProvider, MemoryTokenCacheProvider>();
        services.AddSingleton<IMaskinportenService, MaskinportenService>();

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
                typeof(ITildaDataSource).IsAssignableFrom(type) &&
                type != typeof(TildaDataSource))
            .ToList()
            .ForEach(type => services.AddTransient(typeof(ITildaDataSource), type));

        services.AddSingleton<ITildaSourceProvider, TildaSourceProvider>();

        // Per-host connection cap shared by every outbound pool below. WSAENOBUFS ("system lacked
        // sufficient buffer space") is a process-global socket-buffer limit, so it surfaces on
        // whichever host the worker happens to be dialing when buffers run dry (seen on both
        // data.brreg.no and maskinporten.no). The per-request fan-out (~26 sources + bounded BR
        // enrichment) caps a single request, but nothing bounded the pools across concurrent
        // requests: without this, in-flight requests open connections without limit until the
        // worker exhausts its buffers. Capping per host turns cross-request overload into
        // backpressure (requests wait for a free connection) instead of socket exhaustion.
        const int maxConnectionsPerHost = 50;

        // Appends to Dan.Common's SafeHttpClient registration (its registry-based circuit
        // breaker policy is replaced with a no-op after host build below):
        //
        // - Pooled connection lifetime: TildaDataSource instances are captured by the singleton
        //   TildaSourceProvider, so their HttpClients (and handler chains) live for the process
        //   lifetime and the factory's handler rotation never applies. Recycling pooled
        //   connections ensures upstream DNS changes (failover, traffic manager) are picked up.
        //
        // - Per-host circuit breaker: each upstream authority (scheme://host:port) gets its own
        //   breaker state, so a failing tilsynsmyndighet fails fast without affecting calls to
        //   the other sources.
        services.AddHttpClient("SafeHttpClient")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = maxConnectionsPerHost,
            })
            .AddResilienceHandler("per-host-breaker", builder =>
            {
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,                          // open when >=50% of calls fail...
                    MinimumThroughput = 8,                       // ...but only with enough samples (no tripping on a single blip)
                    SamplingDuration = TimeSpan.FromSeconds(30), // failure ratio measured over this window
                    BreakDuration = TimeSpan.FromSeconds(20),    // fail fast for 20s, then probe again
                });
            })
            .SelectPipelineByAuthority();

        // Also held for the process lifetime by the singleton-captured TildaDataSources
        services.AddHttpClient("AlertHttpClient")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = maxConnectionsPerHost,
            });

        // Default (unnamed) factory client. The singleton MaskinportenService takes a plain
        // HttpClient (its single _client field), which the factory resolves to the default-named
        // client; that is the pool used for token requests to maskinporten.no. Without this it
        // would use the factory's default handler: unbounded MaxConnectionsPerServer and, because
        // the service is a captured singleton, a handler that never recycles (stale DNS on
        // failover). Bound and recycle it like the rest.
        services.AddHttpClient(string.Empty)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = maxConnectionsPerHost,
            });

        // Client configured without circuit breaker policies. shorter timeout
        // brreg's enhetsregisteret normally responds in ~200ms; 10s only trips on a genuine
        // stall, and a stall here is non-fatal (org info is added-value enrichment, see
        // AuditFunctionsBase.GetOrganizationsFromBr).
        services.AddHttpClient("ERHttpClient", client =>
        {
            client.Timeout = new TimeSpan(0, 0, 10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = maxConnectionsPerHost,
        });

        services.AddHttpClient("KofuviClient", client =>
            {
                client.Timeout = new TimeSpan(0, 0, 5);
            })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler { MaxConnectionsPerServer = maxConnectionsPerHost };
            handler.ClientCertificates.Add(Settings.KofuviCertificate);
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

// Dan.Common wires SafeHttpClient/PluginHttpClient with a single circuit breaker whose state is
// shared across all upstream hosts, meaning a few failures from one tilsynsmyndighet would open
// the circuit for every other source. Replace it with a no-op; the per-host circuit breaker
// registered above provides fail-fast per upstream instead, and calls are bounded by the
// SafeHttpClientTimeout app setting.
host.Services.GetRequiredService<IPolicyRegistry<string>>()["SafeHttpClientPolicy"] =
    Policy.NoOpAsync<HttpResponseMessage>();

await host.RunAsync();
