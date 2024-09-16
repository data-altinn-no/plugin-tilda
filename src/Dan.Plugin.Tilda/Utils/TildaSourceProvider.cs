using System.Collections.Generic;
using System.Linq;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Utils;

public interface ITildaSourceProvider
{
    IEnumerable<TildaDataSource> GetAllSources<T>() where T : ITildaEvidenceType;
    IEnumerable<TildaDataSource> GetRelevantSources<T>(string sourceFilter) where T : ITildaEvidenceType;
    IEnumerable<TildaDataSource> GetAllRegisteredSources();
}

public class TildaSourceProvider : ITildaSourceProvider
{
    private readonly IEnumerable<ITildaDataSource> _dataSources;
    private readonly Settings _settings;

    public TildaSourceProvider(IEnumerable<ITildaDataSource> dataSources, IOptions<Settings> settings)
    {
        _dataSources = dataSources;
        _settings = settings.Value;
    }

    public IEnumerable<TildaDataSource> GetAllSources<T>() where T : ITildaEvidenceType
    {
        return _dataSources
            .Where(ds => (_settings.IsTest || !ds.TestOnly) && ds is T)
            .Select(ds => ds as TildaDataSource)
            .ToList();
    }

    public IEnumerable<TildaDataSource> GetRelevantSources<T>(string sourceFilter) where T : ITildaEvidenceType
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
