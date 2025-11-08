// Dtos/EggDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateEggDto(
    string FarmId,
    string ShedId,
    string Date,                // "yyyy-MM-dd"
    int EggsCollected,
    int BrokenEggs = 0,
    string? Notes = null,
    string? LocationName = null,
    double? Lat = null,
    double? Lng = null
);

public record EggDto(
    string Id,
    string FarmId,
    string ShedId,
    string Date,
    int EggsCollected,
    int BrokenEggs,
    string? Notes,
    string? LocationName,
    double? Lat,
    double? Lng,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static EggDto From(EggCollection e) => new(
        e.id, e.FarmId, e.ShedId, e.Date.ToString("yyyy-MM-dd"),
        e.EggsCollected, e.BrokenEggs, e.Notes, e.LocationName, e.Lat, e.Lng,
        e.CreatedUtc, e.UpdatedUtc
    );
}
