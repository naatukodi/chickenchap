// Infrastructure/CosmosRepository.cs
using Microsoft.Azure.Cosmos;
using System.Net;

namespace ChickenChap.Api.Infrastructure;

public sealed class CosmosRepository<T>(Container container) : ICosmosRepository<T>
{
    private readonly Container _container = container;

    public async Task<T> CreateAsync(T entity, string pk)
    {
        var resp = await _container.CreateItemAsync(entity, new PartitionKey(pk));
        return resp.Resource;
    }
    public async Task<T?> GetAsync(string id, string pk)
    {
        try
        {
            var resp = await _container.ReadItemAsync<T>(id, new PartitionKey(pk));
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        { return default; }
    }
    public async IAsyncEnumerable<T> QueryAsync(string query, params (string Name, object Value)[] parameters)
    {
        var qd = new QueryDefinition(query);
        foreach (var p in parameters) qd = qd.WithParameter(p.Name, p.Value);

        using var it = _container.GetItemQueryIterator<T>(qd);
        while (it.HasMoreResults)
        {
            var page = await it.ReadNextAsync();
            foreach (var item in page) yield return item;
        }
    }
    public async Task<T> UpsertAsync(T entity, string pk)
    {
        var resp = await _container.UpsertItemAsync(entity, new PartitionKey(pk));
        return resp.Resource;
    }
    public Task DeleteAsync(string id, string pk)
        => _container.DeleteItemAsync<T>(id, new PartitionKey(pk));
}
