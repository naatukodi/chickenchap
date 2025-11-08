// Domain/EggCollection.cs
namespace ChickenChap.Api.Domain;

public sealed class EggCollection : BaseEntity
{
    public EggCollection() { docType = "egg"; } // set discriminator
    public string FarmId { get; set; } = default!;
    public string ShedId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public int EggsCollected { get; set; }
    public int BrokenEggs { get; set; }
    public string? Notes { get; set; }
    public string? LocationName { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}
