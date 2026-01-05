// Infrastructure/CosmosOptions.cs
namespace ChickenChap.Api.Infrastructure;

public sealed class CosmosOptions
{
    public string Endpoint { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string Database { get; set; } = default!;
    public string Container { get; set; } = "cc";
    public string PartitionKeyPath { get; set; } = "/pk";
    public string? ProvisioningMode { get; set; } = "Serverless"; // or "Provisioned"
}
public sealed class BlobStorageOptions
{
    public string ConnectionString { get; set; } = default!;
    public string ContainerName { get; set; } = "expenses"; // blob container for images
}