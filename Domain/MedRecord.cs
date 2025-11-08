// Domain/MedRecord.cs
namespace ChickenChap.Api.Domain;

public sealed class MedRecord : BaseEntity
{
    public MedRecord() { docType = "med"; }
    public string FarmId { get; set; } = default!;
    public string ShedId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string Name { get; set; } = default!;     // medicine/vaccine name
    public string Type { get; set; } = "Medicine";   // Medicine | Vaccine
    public decimal? DosePerBird { get; set; }
    public int? BirdsTreated { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Notes { get; set; }
}
