// Dtos/FeedDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateFeedDto(
    string FarmId,
    string ShedId,
    string Date,           // yyyy-MM-dd
    string FeedType,
    decimal QuantityKg,
    decimal? CostPerKg,
    string? Notes
);

public record FeedDto(
    string Id,
    string FarmId,
    string ShedId,
    string Date,
    string FeedType,
    decimal QuantityKg,
    decimal? CostPerKg,
    decimal? TotalCost,
    string? Notes,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static FeedDto From(FeedUsage f) => new(
        f.id, f.FarmId, f.ShedId, f.Date.ToString("yyyy-MM-dd"), f.FeedType, f.QuantityKg, f.CostPerKg,
        f.CostPerKg.HasValue ? f.CostPerKg * f.QuantityKg : null, f.Notes, f.CreatedUtc, f.UpdatedUtc
    );
}
