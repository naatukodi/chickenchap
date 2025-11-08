// Dtos/MedDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateMedDto(
    string FarmId,
    string ShedId,
    string Date,
    string Name,
    string Type,              // Medicine|Vaccine
    decimal? DosePerBird,
    int? BirdsTreated,
    decimal? TotalCost,
    string? Notes
);

public record MedDto(
    string Id,
    string FarmId,
    string ShedId,
    string Date,
    string Name,
    string Type,
    decimal? DosePerBird,
    int? BirdsTreated,
    decimal? TotalCost,
    string? Notes,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static MedDto From(MedRecord m) => new(
        m.id, m.FarmId, m.ShedId, m.Date.ToString("yyyy-MM-dd"), m.Name, m.Type,
        m.DosePerBird, m.BirdsTreated, m.TotalCost, m.Notes, m.CreatedUtc, m.UpdatedUtc
    );
}
