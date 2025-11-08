// Domain/FeedUsage.cs
namespace ChickenChap.Api.Domain;

public sealed class FeedUsage : BaseEntity
{
    public FeedUsage() { docType = "feed"; }
    public string FarmId { get; set; } = default!;
    public string ShedId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string FeedType { get; set; } = default!; // Starter/Grower/Layer/...
    public decimal QuantityKg { get; set; }
    public decimal? CostPerKg { get; set; }
    public string? Notes { get; set; }
}
