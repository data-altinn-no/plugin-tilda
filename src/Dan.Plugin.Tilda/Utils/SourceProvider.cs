using System.Collections.Generic;
using System.Linq;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Utils;

public interface ISourceProvider
{
    IEnumerable<TildaDataSource> GetAllSources<T>() where T : ITildaSource;
    IEnumerable<TildaDataSource> GetRelevantSources<T>(string sourceFilter) where T : ITildaSource;
    IEnumerable<TildaDataSource> GetAllRegisteredSources();
}

public class SourceProvider : ISourceProvider
{
    private readonly IEnumerable<ITildaDataSource> _dataSources;
    private readonly Settings _settings;

    public SourceProvider(IEnumerable<ITildaDataSource> dataSources, IOptions<Settings> settings)
    {
        _dataSources = dataSources;
        _settings = settings.Value;
    }

    public IEnumerable<TildaDataSource> GetAllSources<T>() where T : ITildaSource
    {
        return _dataSources
            .Where(ds => (_settings.IsTest || !ds.TestOnly) && ds is T)
            .Select(ds => ds as TildaDataSource)
            .ToList();
    }

    public IEnumerable<TildaDataSource> GetRelevantSources<T>(string sourceFilter) where T : ITildaSource
    {
        return GetAllSources<T>()
            .Where(t => sourceFilter.Contains(t.OrganizationNumber))
            .ToList();
    }

    public IEnumerable<TildaDataSource> GetAllRegisteredSources()
    {
        return _dataSources
            .Where(ds => _settings.IsTest || !ds.TestOnly)
            .Select(ds => ds as TildaDataSource)
            .ToList();
    }
}
