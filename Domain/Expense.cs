// Domain/Expense.cs
namespace ChickenChap.Api.Domain;

public sealed class Expense : BaseEntity
{
    public Expense() { docType = "expense"; }
    public string FarmId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string Category { get; set; } = default!; // Feed/Medicine/Labor/...
    public string? Vendor { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? ReceiptLink { get; set; }
    public string? Notes { get; set; }
}
