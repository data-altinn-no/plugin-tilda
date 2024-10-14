using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Tilda.Clients;

public interface IMtamCounterClient
{
    Task<MtamCounter> GetMtamCounter(string organizationNumber);
    Task UpsertMtamCounter(MtamCounter mtamCounter);
}

public class MtamCounterClient : IMtamCounterClient
{
    private readonly Container _container;
    private const string MtamCounterContainerName = "MtamCounter";

    public MtamCounterClient(
        CosmosClient cosmosClient,
        IOptions<Settings> settings)
    {
        _container = cosmosClient.GetContainer(settings.Value.CosmosDbDatabase, MtamCounterContainerName);
    }

    public async Task<MtamCounter> GetMtamCounter(string organizationNumber)
    {
        MtamCounter mtamCounters;
        try
        {
            mtamCounters = await _container.ReadItemAsync<MtamCounter>(organizationNumber, new PartitionKey(organizationNumber));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            mtamCounters = new MtamCounter
            {
                Id = organizationNumber,
                LastFetched = DateTime.MinValue,
            };
        }

        return mtamCounters;
    }

    public async Task UpsertMtamCounter(MtamCounter mtamCounter)
    {
        await _container.UpsertItemAsync(mtamCounter);
    }
}
