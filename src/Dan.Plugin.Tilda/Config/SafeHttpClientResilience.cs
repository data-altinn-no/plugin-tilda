using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Dan.Plugin.Tilda.Config;

public static class SafeHttpClientResilience
{
    public const string PipelineName = "per-host-breaker";

    /// <summary>
    /// Per-authority (scheme://host:port) resilience for the outbound tilsynsmyndighet calls.
    ///
    /// The per-attempt timeout must live INSIDE the pipeline: HttpClient.Timeout cancels from
    /// outside the handler chain, so the circuit breaker only ever saw an
    /// OperationCanceledException, which it does not count as a failure. That is why a source
    /// timing out on 100% of calls never opened the breaker in production. With the timeout
    /// inside the pipeline the breaker observes TimeoutRejectedException and opens per host.
    ///
    /// Strategy order matters: Polly executes strategies in registration order, outermost first,
    /// so the breaker is added before the timeout so it wraps (and observes) it.
    ///
    /// The inner timeout covers time-to-response-headers (the handler chain). A stalled body is
    /// still bounded only by the outer HttpClient.Timeout (SafeHttpClientTimeout), which must be
    /// strictly longer than <paramref name="attemptTimeout"/> or the outer cancellation wins and
    /// the breaker is blind again.
    /// </summary>
    public static IHttpClientBuilder AddPerHostResilience(this IHttpClientBuilder builder, TimeSpan attemptTimeout)
    {
        builder
            .AddResilienceHandler(PipelineName, pipeline =>
            {
                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,                          // open when >=50% of calls fail...
                    MinimumThroughput = 8,                       // ...but only with enough samples (no tripping on a single blip)
                    SamplingDuration = TimeSpan.FromSeconds(30), // failure ratio measured over this window
                    BreakDuration = TimeSpan.FromSeconds(20),    // fail fast for 20s, then probe again
                });
                pipeline.AddTimeout(attemptTimeout);             // throws TimeoutRejectedException, counted by the breaker
            })
            .SelectPipelineByAuthority();

        return builder;
    }
}
