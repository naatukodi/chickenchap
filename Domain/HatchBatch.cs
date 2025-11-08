// Domain/HatchBatch.cs
namespace ChickenChap.Api.Domain;

public sealed class HatchBatch : BaseEntity
{
    public HatchBatch() { docType = "hatch"; }
    public string FarmId { get; set; } = default!;
    public string BatchId { get; set; } = default!;
    public string? Breed { get; set; }
    public string? IncubatorType { get; set; }
    public DateOnly SetDate { get; set; }
    public int EggsSet { get; set; }
    public int? Infertile { get; set; }
    public int? EarlyMortality { get; set; }
    public int? LateMortality { get; set; }
    public int? Hatched { get; set; }
    public string? Notes { get; set; }
}
