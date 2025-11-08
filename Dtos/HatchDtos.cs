// Dtos/HatchDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateHatchDto(
    string FarmId,
    string BatchId,
    string SetDate,
    string? Breed,
    string? IncubatorType,
    int EggsSet,
    int? Infertile,
    int? EarlyMortality,
    int? LateMortality,
    int? Hatched,
    string? Notes
);

public record HatchDto(
    string Id,
    string FarmId,
    string BatchId,
    string SetDate,
    string? Breed,
    string? IncubatorType,
    int EggsSet,
    int? Infertile,
    int? EarlyMortality,
    int? LateMortality,
    int? Hatched,
    double? HatchRate,               // computed
    string? Notes,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static HatchDto From(HatchBatch h)
    {
        double? rate = (h.Hatched.HasValue && h.EggsSet > 0)
            ? (double)h.Hatched.Value / h.EggsSet
            : null;
        return new(
            h.id, h.FarmId, h.BatchId, h.SetDate.ToString("yyyy-MM-dd"), h.Breed, h.IncubatorType,
            h.EggsSet, h.Infertile, h.EarlyMortality, h.LateMortality, h.Hatched, rate,
            h.Notes, h.CreatedUtc, h.UpdatedUtc
        );
    }
}
