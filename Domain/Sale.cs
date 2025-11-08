// Domain/Sale.cs
namespace ChickenChap.Api.Domain;

public enum SaleCategory { Eggs, Chicks, Other }

public sealed class Sale : BaseEntity
{
    public Sale() { docType = "sale"; }
    public string FarmId { get; set; } = default!;
    public SaleCategory Category { get; set; }
    public string? ItemOrBreed { get; set; }
    public string? Buyer { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateOnly Date { get; set; }
    public string? PaymentMode { get; set; }
    public int? ChickAgeDays { get; set; }
    public string? Notes { get; set; }
}
