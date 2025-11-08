using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace ChickenChap.Api.Infrastructure;

public interface ICosmosClientFactory
{
    CosmosClient Client { get; }
    Database Database { get; }
    Container Container { get; }
    Task EnsureProvisionedAsync();
}

public sealed class CosmosClientFactory(IOptions<CosmosOptions> opts) : ICosmosClientFactory
{
    private readonly CosmosOptions _opts = opts.Value;
    private readonly CosmosClient _client =
        new CosmosClient(opts.Value.Endpoint, opts.Value.Key,
            new CosmosClientOptions { ApplicationName = "ChickenChap.Api" });

    public CosmosClient Client => _client;
    public Database Database => _client.GetDatabase(_opts.Database);
    public Container Container => Database.GetContainer(_opts.Container);

    public async Task EnsureProvisionedAsync()
    {
        var isServerless = string.Equals(_opts.ProvisioningMode, "Serverless",
            StringComparison.OrdinalIgnoreCase);

        // 1) Create database
        if (isServerless)
        {
            // ❌ No throughput on serverless
            await _client.CreateDatabaseIfNotExistsAsync(_opts.Database);
        }
        else
        {
            // ✅ Provisioned or Autoscale (choose one)
            // Example provisioned at DB level (shared):
            await _client.CreateDatabaseIfNotExistsAsync(_opts.Database, throughput: 400);
        }

        // 2) Create container
        var props = new ContainerProperties(_opts.Container, _opts.PartitionKeyPath)
        {
            IndexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent
            }
        };

        // Optional composite index for fast ORDER BY on Date within a farm:
        props.IndexingPolicy.CompositeIndexes.Add(new System.Collections.ObjectModel.Collection<CompositePath>
        {
            new CompositePath { Path = "/pk",   Order = CompositePathSortOrder.Ascending },
            new CompositePath { Path = "/Date", Order = CompositePathSortOrder.Descending }
        });

        if (isServerless)
        {
            // ❌ No throughput on serverless
            await Database.CreateContainerIfNotExistsAsync(props);
        }
        else
        {
            // ✅ Provisioned throughput at container level (if not using DB shared throughput)
            // Pick ONE of these; comment out if using shared DB throughput above:
            await Database.CreateContainerIfNotExistsAsync(props /*, throughput: 400 */);
            // or autoscale:
            // await Database.CreateContainerIfNotExistsAsync(props, ThroughputProperties.CreateAutoscaleThroughput(4000));
        }
    }
}
