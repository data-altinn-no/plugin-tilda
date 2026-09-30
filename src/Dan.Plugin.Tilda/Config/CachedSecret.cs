using System;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Config;

/// <summary>
/// Thread-safe, asynchronous, load-once cache for a secret-backed value. Concurrent first
/// callers share one in-flight load (no thundering herd on a cold instance), and a faulted
/// or cancelled load is discarded so the next caller retries instead of the process being
/// poisoned by a transient Key Vault error.
/// </summary>
internal sealed class CachedSecret<T>
{
    private readonly object _gate = new();
    private Task<T> _task;

    public Task<T> GetAsync(Func<Task<T>> factory)
    {
        lock (_gate)
        {
            if (_task is null || _task.IsFaulted || _task.IsCanceled)
            {
                _task = Task.Run(factory);
            }

            return _task;
        }
    }

    /// <summary>Pre-seed the cache (tests, or values supplied without a secret store).</summary>
    public void Set(T value)
    {
        lock (_gate)
        {
            _task = Task.FromResult(value);
        }
    }
}
