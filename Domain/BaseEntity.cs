// Domain/BaseEntity.cs
namespace ChickenChap.Api.Domain;

public abstract class BaseEntity
{
    public string id { get; set; } = Guid.NewGuid().ToString("N");
    public string pk { get; set; } = default!;    // partition key (FarmId)
    public string docType { get; set; } = default!; // discriminator: "egg","sale","feed","med","expense","hatch"
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
